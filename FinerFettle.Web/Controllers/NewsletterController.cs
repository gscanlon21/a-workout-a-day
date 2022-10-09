using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FinerFettle.Web.Data;
using FinerFettle.Web.Models.User;
using FinerFettle.Web.Models.Exercise;
using FinerFettle.Web.ViewModels.Newsletter;
using FinerFettle.Web.Extensions;
using FinerFettle.Web.Models.Newsletter;
using System.Numerics;
using System.Linq;

namespace FinerFettle.Web.Controllers
{
    public class NewsletterController : Controller
    {
        private readonly CoreContext _context;

        /// <summary>
        /// The name of the controller for routing purposes
        /// </summary>
        public const string Name = "Newsletter";

        public NewsletterController(CoreContext context)
        {
            _context = context;
        }

        [Route("newsletter/{email}")]
        public async Task<IActionResult> Newsletter(string email, bool demo = false)
        {
            var today = DateOnly.FromDateTime(DateTime.Today);

            var user = await _context.Users
                // For displaying ignored exercises in the bottom of the newsletter
                .Include(u => u.UserExercises)
                    .ThenInclude(ep => ep.Exercise)
                // For displaying user's equipment in the bottom of the newsletter
                .Include(u => u.UserEquipments)
                    .ThenInclude(u => u.Equipment)
                .FirstAsync(u => u.Email == email);
                
            if (user.Disabled || user.RestDays.HasFlag(RestDaysExtensions.FromDate(today)))
            {
                return NoContent();
            }

            var previousNewsletter = await _context.Newsletters
                .Where(n => n.User == user)
                .OrderBy(n => n.Date)
                .ThenBy(n => n.Id) // For testing/demo. When two newsletters get sent in the same day, I want a different exercise set.
                .LastOrDefaultAsync();

            var todoExerciseType = new ExerciseTypeGroups(user.StrengtheningPreference).First(); // Have to start somewhere
            if (previousNewsletter != null)
            {
                todoExerciseType = new ExerciseTypeGroups(user.StrengtheningPreference)
                    .SkipWhile(r => r != previousNewsletter.ExerciseRotation)
                    .Skip(1)
                    .FirstOrDefault() ?? todoExerciseType;
            }

            var lastDeload = await _context.Newsletters
                .Where(n => n.User == user)
                .OrderBy(n => n.Date)
                .ThenBy(n => n.Id) // For testing/demo. When two newsletters are sent the same day, I want a different exercise set.
                .LastOrDefaultAsync(n => n.IsDeloadWeek) 
                    ?? await _context.Newsletters
                    .Where(n => n.User == user)
                    .OrderBy(n => n.Date)
                    .ThenBy(n => n.Id) // For testing/demo. When two newsletters are sent the same day, I want a different exercise set.
                    .FirstOrDefaultAsync(); // The oldest newsletter, for if there has never been a deload before.
            
            // Deloads are weeks with a message to lower the intensity of the workout so muscle growth doesn't stagnate
            bool needsDeload = lastDeload != null 
                && ( 
                    // Dates are the same week. Keep the deload going until the week is over.
                    (lastDeload.IsDeloadWeek && lastDeload.Date.AddDays(-1 * (int)lastDeload.Date.DayOfWeek) == today.AddDays(-1 * (int)today.DayOfWeek))
                    // Or the last deload/oldest newsletter was 1+ months ago
                    || lastDeload.Date.AddMonths(1) < today 
                );

            var newsletter = new Newsletter()
            {
                IsDeloadWeek = needsDeload,
                Date = today,
                User = user,
                ExerciseRotation = todoExerciseType
            };
            _context.Newsletters.Add(newsletter);
            await _context.SaveChangesAsync();

            // Flatten all user-eligible exercise variations into one big list
            var allExercises = (await _context.Variations
                .Include(i => i.Intensities)
                .Include(v => v.Exercise)
                .Include(i => i.EquipmentGroups)
                    // To display the equipment required for the exercise in the newsletter
                    .ThenInclude(eg => eg.Equipment)
                // Select the current progression of each exercise
                .Select(i => new {
                    Variation = i,
                    UserVariation = i.UserVariations.FirstOrDefault(uv => uv.User == user),
                    UserExercise = i.Exercise.UserExercises.FirstOrDefault(ue => ue.User == user)
                })
                // Don't grab exercises that the user wants to ignore
                .Where(i => i.UserExercise == null || !i.UserExercise.Ignore)
                // Only show these exercises if the user has completed the previous reqs
                .Where(i => i.Variation.Exercise.Prerequisites
                                .Select(r => new { r.PrerequisiteExercise.Proficiency, UserExercise = r.PrerequisiteExercise.UserExercises.FirstOrDefault(up => up.User == user) })
                                .All(p => p.UserExercise == null || p.UserExercise.Ignore || p.UserExercise.Progression >= p.Proficiency)
                )
                // Hide these exercises if the user has achieved mastery in the post-reqs // I'd rather the user ignore the easier exercise themselves
                //.Where(i => i.Variation.Variation.Exercise.Prerequisites
                //                .Select(r => new { r.Exercise.Proficiency, UserExercise = r.Exercise.UserExercises.FirstOrDefault(up => up.User == user) })
                //                .All(p => p.UserExercise == null || p.UserExercise.Ignore || p.UserExercise.Progression < UserExercise.MaxUserProgression)
                //)
                .Where(i => i.Variation.Progression.Min == null
                                // User hasn't ever seen this exercise before. Show it so an ExerciseUserExercise record is made.
                                || (i.UserExercise == null && (UserExercise.RoundToNearestX * (int)Math.Floor(user.AverageProgression / (double)UserExercise.RoundToNearestX) >= i.Variation.Progression.Min))
                                // Compare the exercise's progression range with the user's exercise progression
                                || (i.UserExercise != null && (UserExercise.RoundToNearestX * (int)Math.Floor(i.UserExercise!.Progression / (double)UserExercise.RoundToNearestX)) >= i.Variation.Progression.Min))
                .Where(i => i.Variation.Progression.Max == null
                                // User hasn't ever seen this exercise before. Show it so an ExerciseUserExercise record is made.
                                || (i.UserExercise == null && (UserExercise.RoundToNearestX * (int)Math.Ceiling(user.AverageProgression / (double)UserExercise.RoundToNearestX) < i.Variation.Progression.Max))
                                // Compare the exercise's progression range with the user's exercise progression
                                || (i.UserExercise != null && (UserExercise.RoundToNearestX * (int)Math.Ceiling(i.UserExercise!.Progression / (double)UserExercise.RoundToNearestX)) < i.Variation.Progression.Max))
                .Where(i => (
                        // User owns at least one equipment in at least one of the optional equipment groups
                        !i.Variation.EquipmentGroups.Any(eg => !eg.Required && eg.Equipment.Any())
                        || i.Variation.EquipmentGroups.Where(eg => !eg.Required && eg.Equipment.Any()).Any(eg => eg.Equipment.Any(e => user.EquipmentIds.Contains(e.Id)))
                    ) && (
                        // User owns at least one equipment in all of the required equipment groups
                        !i.Variation.EquipmentGroups.Any(eg => eg.Required && eg.Equipment.Any())
                        || i.Variation.EquipmentGroups.Where(eg => eg.Required && eg.Equipment.Any()).All(eg => eg.Equipment.Any(e => user.EquipmentIds.Contains(e.Id)))
                    ))
                .ToListAsync()) // OrderBy must come after query or you get duplicates
                // Show exercises that the user has rarely seen
                .OrderBy(a => a.UserExercise == null ? DateOnly.MinValue : a.UserExercise.LastSeen)
                // User prefers weighted variations, order those next
                .ThenByDescending(a => user.PrefersWeights && a.Variation.EquipmentGroups.Any(eg => eg.IsWeight))
                // Show variations that the user has rarely seen
                .ThenBy(a => a.UserVariation == null ? DateOnly.MinValue : a.UserVariation.LastSeen)
                // Mostly for the demo, show mostly random exercises
                .ThenBy(a => Guid.NewGuid())
                .ToList();

            // Main exercises
            var mainExercises = allExercises
                .Select(i => new ExerciseViewModel(user, i.Variation, intensityLevel: null, activityLevel: ExerciseActivityLevel.Main)
                {
                    Demo = demo
                })
                // Make sure the exercise is for the correct workout type
                .Where(vm => vm.Variation.ExerciseType.HasFlag(todoExerciseType.ExerciseType))
                // If a recovery muscle is set, don't choose any exercises that work the injured muscle
                .Where(i => user.RecoveryMuscle == MuscleGroups.None || !i.Variation.Exercise.AllMuscles.HasFlag(user.RecoveryMuscle));
            var exercises = mainExercises
                .Aggregate(new List<ExerciseViewModel>(), (vms, vm) => (
                    // Choose either compound exercises that cover at least two muscles in the targeted muscles set
                    BitOperations.PopCount((ulong)todoExerciseType.MuscleGroups.UnsetFlag32(vm.Exercise.PrimaryMuscles.UnsetFlag32(vms.Aggregate((MuscleGroups)0, (m, vm2) => m | vm2.Exercise.PrimaryMuscles)))) <= (BitOperations.PopCount((ulong)todoExerciseType.MuscleGroups) - 2)
                ) ? new List<ExerciseViewModel>(vms) { vm } : vms);
            exercises = exercises.Concat(mainExercises.Aggregate(new List<ExerciseViewModel>(), (vms, vm) => (
                    // Grab any muscle groups we missed in the previous aggregate. Include isolation exercises here
                    vm.Exercise.PrimaryMuscles.UnsetFlag32(vms.Aggregate(exercises.Aggregate((MuscleGroups)0, (m, vm2) => m | vm2.Exercise.PrimaryMuscles), (m, vm2) => m | vm2.Exercise.PrimaryMuscles)).HasAnyFlag32(todoExerciseType.MuscleGroups)
                    ) ? new List<ExerciseViewModel>(vms) { vm } : vms))
                // Show most complex exercises first
                .OrderByDescending(e => BitOperations.PopCount((ulong)e.Exercise.PrimaryMuscles))
                .ToList();

            var viewModel = new NewsletterViewModel(exercises, user, newsletter)
            {
                ExerciseType = todoExerciseType.ExerciseType,
                MuscleGroups = todoExerciseType.MuscleGroups,
                AllEquipment = new EquipmentViewModel(_context.Equipment, user.UserEquipments.Select(eu => eu.Equipment)),
                Demo = demo
            };

            // Warmup exercises
            var warmupExercises = allExercises
                .Select(a => new ExerciseViewModel(user, a.Variation, IntensityLevel.WarmupCooldown, ExerciseActivityLevel.Warmup)
                {
                    Demo = demo
                })
                // Make sure the exercise is a warmup stretch
                .Where(vm => vm.Variation.ExerciseType.HasAnyFlag32(todoExerciseType.ExerciseType == ExerciseType.Cardio ? ExerciseType.Cardio : ExerciseType.Flexibility | ExerciseType.Cardio))
                // Don't show weighted exercises for warmups
                .Where(vm => !ExerciseViewModel.IsWeighted(vm))
                // Choose dynamic stretches for warmups
                .Where(vm => !ExerciseViewModel.IsIsometric(vm))
                // If a recovery muscle is set, don't choose any exercises that work the injured muscle
                .Where(i => user.RecoveryMuscle == MuscleGroups.None || !i.Variation.Exercise.AllMuscles.HasFlag(user.RecoveryMuscle))
                .ToList();
            var item = warmupExercises.FirstOrDefault(e => e.Variation.ExerciseType.HasFlag(ExerciseType.Cardio) && !e.Variation.MuscleContractions.HasFlag(MuscleContractions.Isometric));
            if (item != null)
            {
                // Need something to get the heart rate up
                warmupExercises.Remove(item);
                warmupExercises.Insert(0, item);
            }
            viewModel.WarmupExercises = warmupExercises
                .Aggregate(new List<ExerciseViewModel>(), (vms, vm) => (
                    // Grab compound exercises that cover at least three muscles in the targeted muscles set
                    BitOperations.PopCount((ulong)viewModel.MuscleGroups.UnsetFlag32(vm.Exercise.PrimaryMuscles.UnsetFlag32(vms.Aggregate((MuscleGroups)0, (m, vm2) => m | vm2.Exercise.PrimaryMuscles)))) <= (BitOperations.PopCount((ulong)viewModel.MuscleGroups) - 3)
                ) ? new List<ExerciseViewModel>(vms) { vm } : vms).ToList();
            viewModel.WarmupExercises = viewModel.WarmupExercises
                .Concat(warmupExercises
                    .Aggregate(new List<ExerciseViewModel>(), (vms, vm) => (
                        // Grab any muscle groups we missed in the previous aggregate
                        vm.Exercise.PrimaryMuscles.UnsetFlag32(vms.Aggregate(viewModel.WarmupExercises.Aggregate((MuscleGroups)0, (m, vm2) => m | vm2.Exercise.PrimaryMuscles), (m, vm2) => m | vm2.Exercise.PrimaryMuscles)).HasAnyFlag32(viewModel.MuscleGroups)
                    ) ? new List<ExerciseViewModel>(vms) { vm } : vms))
                // Move the exercises that get the heart rate up to the end
                .OrderBy(e => e.Variation.ExerciseType.HasFlag(ExerciseType.Cardio))
                .ToList();

            // Recovery exercises
            if (user.RecoveryMuscle != MuscleGroups.None)
            {
                viewModel.RecoveryExercises = allExercises
                    .Select(i => new ExerciseViewModel(user, i.Variation, IntensityLevel.WarmupCooldown, ExerciseActivityLevel.Warmup)
                    {
                        Demo = demo
                    })
                    // Make sure the exercise is a warmup stretch
                    .Where(vm => vm.Variation.ExerciseType.HasFlag(ExerciseType.Flexibility))
                    // Choose dynamic stretches for warmups
                    .Where(vm => !ExerciseViewModel.IsIsometric(vm))
                    // Don't show weighted exercises for warmups
                    .Where(vm => !ExerciseViewModel.IsWeighted(vm))
                    // Choose recovery exercises that work the recovery muscle
                    .Where(i => i.Variation.Exercise.PrimaryMuscles.HasFlag(user.RecoveryMuscle))
                    .Take(1)
                    .Concat(allExercises
                        .Select(i => new ExerciseViewModel(user, i.Variation, IntensityLevel.Recovery, ExerciseActivityLevel.Main)
                        {
                            Demo = demo,
                        })
                        // Make sure this is a recovery-level exercise // I don't know if I need this with exercise and intensity progressions.
                        //.Where(e => e.Variation.Intensities.Any(ip => ip.IntensityLevel == IntensityLevel.Recovery))
                        // Make sure this is a strengthening exercise
                        .Where(vm => vm.Variation.ExerciseType.HasFlag(ExerciseType.Strength))
                        // Choose recovery exercises that work the recovery muscle
                        .Where(i => i.Variation.Exercise.PrimaryMuscles.HasFlag(user.RecoveryMuscle))
                        .Take(1))
                    .Concat(allExercises
                        .Select(i => new ExerciseViewModel(user, i.Variation, IntensityLevel.WarmupCooldown, ExerciseActivityLevel.Cooldown)
                        {
                            Demo = demo
                        })
                        // Make sure the exercise is a cooldown stretch
                        .Where(vm => vm.Variation.ExerciseType.HasFlag(ExerciseType.Flexibility))
                        // Choose static stretches for cooldowns
                        .Where(ExerciseViewModel.IsIsometric)
                        // Don't show weighted exercises for cooldowns
                        .Where(vm => !ExerciseViewModel.IsWeighted(vm))
                        // Choose recovery exercises that work the recovery muscle
                        .Where(i => i.Variation.Exercise.PrimaryMuscles.HasFlag(user.RecoveryMuscle))
                        .Take(1))
                    .ToList();
            }

            // Sports exercises
            if (user.SportsFocus != SportsFocus.None && !newsletter.IsDeloadWeek)
            {
                var enduranceIntensityLevel = viewModel.ExerciseType == ExerciseType.Cardio ? IntensityLevel.Endurance : IntensityLevel.Gain;
                viewModel.SportsExercises = allExercises
                    .Select(i => new ExerciseViewModel(user, i.Variation, enduranceIntensityLevel, ExerciseActivityLevel.Main)
                    {
                        Demo = demo,
                    })
                    // Make sure the exercise is for the correct workout type
                    .Where(vm => vm.Variation.ExerciseType.HasFlag(viewModel.ExerciseType))
                    // Choose recovery exercises that work the sports muscle
                    .Where(i => i.Variation.SportsFocus.HasFlag(user.SportsFocus))
                    .Take(3)
                    // Show most complex exercises first
                    .OrderByDescending(e => BitOperations.PopCount((ulong)e.Variation.Exercise.PrimaryMuscles))
                    .ToList();
            }

            // Cooldown exercises
            var cooldownExercises = allExercises
                .Select(i => new ExerciseViewModel(user, i.Variation, IntensityLevel.WarmupCooldown, ExerciseActivityLevel.Cooldown)
                {
                    Demo = demo
                })
                // Make sure the exercise is a cooldown stretch
                .Where(vm => vm.Variation.ExerciseType.HasFlag(ExerciseType.Flexibility))
                // Choose static stretches for cooldowns
                .Where(ExerciseViewModel.IsIsometric)
                // Don't show weighted exercises for cooldowns
                .Where(vm => !ExerciseViewModel.IsWeighted(vm))
                // If a recovery muscle is set, don't choose any exercises that work the injured muscle
                .Where(i => user.RecoveryMuscle == MuscleGroups.None || !i.Variation.Exercise.AllMuscles.HasFlag(user.RecoveryMuscle));
            viewModel.CooldownExercises = cooldownExercises
                .Aggregate(new List<ExerciseViewModel>(), (vms, vm) => (
                    // Grab compound exercises that cover at least three muscles in the targeted muscles set
                    BitOperations.PopCount((ulong)viewModel.MuscleGroups.UnsetFlag32(vm.Exercise.AllMuscles.UnsetFlag32(vms.Aggregate((MuscleGroups)0, (m, vm2) => m | vm2.Exercise.AllMuscles)))) <= (BitOperations.PopCount((ulong)viewModel.MuscleGroups) - 3)
                ) ? new List<ExerciseViewModel>(vms) { vm } : vms);
            viewModel.CooldownExercises = viewModel.CooldownExercises
                .Concat(cooldownExercises
                    .Aggregate(new List<ExerciseViewModel>(), (vms, vm) => (
                        // Grab any muscle groups we missed in the previous aggregate
                        vm.Exercise.AllMuscles.UnsetFlag32(vms.Aggregate(viewModel.CooldownExercises.Aggregate((MuscleGroups)0, (m, vm2) => m | vm2.Exercise.AllMuscles), (m, vm2) => m | vm2.Exercise.AllMuscles)).HasAnyFlag32(viewModel.MuscleGroups)
                    ) ? new List<ExerciseViewModel>(vms) { vm } : vms))
                .ToList();

            return View(nameof(Newsletter), viewModel);
        }
    }
}
