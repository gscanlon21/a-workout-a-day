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
                .Include(u => u.UserExercises)
                    .ThenInclude(ep => ep.Exercise)
                .Include(u => u.EquipmentUsers)
                    .ThenInclude(u => u.Equipment)
                .FirstAsync(u => u.Email == email);
                
            if (user.Disabled || user.RestDays.HasFlag(RestDaysExtensions.FromDate(today)))
            {
                return NoContent();
            }

            var todoExerciseType = new ExerciseTypeGroups(user.StrengtheningPreference).First(); // Have to start somewhere
            
            var previousNewsletter = await _context.Newsletters
                .Where(n => n.User == user)
                .OrderBy(n => n.Date)
                .ThenBy(n => n.Id) // For testing/demo. When two newsletters get sent in the same day, I want a different exercise set.
                .LastOrDefaultAsync();

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
                .ThenBy(n => n.Id) // For testing/demo. When two newsletters get sent in the same day, I want a different exercise set.
                .LastOrDefaultAsync(n => n.IsDeloadWeek) 
                    ?? await _context.Newsletters
                    .Where(n => n.User == user)
                    .OrderBy(n => n.Date)
                    .ThenBy(n => n.Id) // For testing/demo. When two newsletters get sent in the same day, I want a different exercise set.
                    .FirstOrDefaultAsync(); // The oldest newsletter, for if there has never been a deload before.

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

            // Flatten all exercise variations and intensities into one big list
            var allExercises = (await _context.Variations
                .Include(v => v.UserVariations.Where(uv => uv.User == user).Take(1))
                .Include(i => i.Intensities)
                .Include(i => i.EquipmentGroups)
                    .ThenInclude(eg => eg.Equipment)
                .Include(v => v.Exercise)
                    .ThenInclude(e => e.UserExercises.Where(up => up.User == user).Take(1))
                .Include(v => v.Exercise)
                    .ThenInclude(e => e.Prerequisites)
                // Select the current progression of each exercise.
                .Select(i => new {
                    Variation = i,
                    UserVariation = i.UserVariations.SingleOrDefault(),
                    UserExercise = i.Exercise.UserExercises.SingleOrDefault()
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
                //                .All(p => p.UserExercise == null || p.UserExercise.Ignore || p.UserExercise.Progression < 95)
                //)
                // Using averageProgression as a boost so that users can't get stuck without an exercise if they never see it because they are under the exercise's min progression
                .Where(i => i.Variation.Progression.Min == null
                                // User hasn't ever seen this exercise before. Show it so an ExerciseUserExercise record is made.
                                || (i.UserExercise == null && (5 * (int)Math.Floor(user.AverageProgression / 5d) >= i.Variation.Progression.Min))
                                // Compare the exercise's progression range with the average of the user's average progression and the user's exercise progression
                                || (i.UserExercise != null && (5 * (int)Math.Floor((user.AverageProgression + i.UserExercise!.Progression) / 10d)) >= i.Variation.Progression.Min))
                .Where(i => i.Variation.Progression.Max == null
                                // User hasn't ever seen this exercise before. Show it so an ExerciseUserExercise record is made.
                                || (i.UserExercise == null && (5 * (int)Math.Ceiling(user.AverageProgression / 5d) < i.Variation.Progression.Max))
                                // Compare the exercise's progression range with the average of the user's average progression and the user's exercise progression
                                || (i.UserExercise != null && (5 * (int)Math.Ceiling((user.AverageProgression + i.UserExercise!.Progression) / 10d)) < i.Variation.Progression.Max))
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
                // Make sure the exercise is for the correct workout type
                .Where(vm => vm.Variation.ExerciseType.HasFlag(todoExerciseType.ExerciseType))
                // If a recovery muscle is set, don't choose any exercises that work the injured muscle
                .Where(i => user.RecoveryMuscle == MuscleGroups.None || !i.Variation.Exercise.AllMuscles.HasFlag(user.RecoveryMuscle))
                .Select(i => new ExerciseViewModel(user, i.Variation, intensityLevel: null, activityLevel: ExerciseActivityLevel.Main)
                {
                    Demo = demo
                });
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
                AllEquipment = new EquipmentViewModel(_context.Equipment, user.EquipmentUsers.Select(eu => eu.Equipment)),
                Demo = demo
            };

            // Warmup exercises
            var warmupExercises = allExercises
                // Make sure the exercise is a warmup stretch
                .Where(vm => vm.Variation.ExerciseType.HasAnyFlag32(todoExerciseType.ExerciseType == ExerciseType.Cardio ? ExerciseType.Cardio : ExerciseType.Flexibility | ExerciseType.Cardio))
                // Don't show weighted exercises for warmups
                .Where(a => !a.Variation.EquipmentGroups.Any(eg => eg.IsWeight))
                // Choose dynamic stretches for warmups
                .Where(e => !e.Variation.MuscleContractions.HasFlag(MuscleContractions.Isometric))
                // If a recovery muscle is set, don't choose any exercises that work the injured muscle
                .Where(i => user.RecoveryMuscle == MuscleGroups.None || !i.Variation.Exercise.AllMuscles.HasFlag(user.RecoveryMuscle))
                .Select(i => new ExerciseViewModel(user, i.Variation, IntensityLevel.WarmupCooldown, ExerciseActivityLevel.Warmup)
                {
                    Demo = demo
                }).ToList();
            var item = warmupExercises.FirstOrDefault(e => e.Variation.ExerciseType.HasFlag(ExerciseType.Cardio) && !e.Variation.MuscleContractions.HasFlag(MuscleContractions.Isometric));
            if (item != null)
            {
                // Need something to get the heart rate up
                warmupExercises.Remove(item);
                warmupExercises.Insert(0, item);
            }
            viewModel.WarmupExercises = warmupExercises
                .Aggregate(new List<ExerciseViewModel>(), (vms, vm) => (
                    // Grab compound exercises that cover at least two muscles in the targeted muscles set
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
                    // Make sure the exercise is a warmup stretch
                    .Where(vm => vm.Variation.ExerciseType.HasFlag(ExerciseType.Flexibility))
                    // Choose dynamic stretches for warmups
                    .Where(e => !e.Variation.MuscleContractions.HasFlag(MuscleContractions.Isometric))
                    // Choose recovery exercises that work the recovery muscle
                    .Where(i => i.Variation.Exercise.PrimaryMuscles.HasFlag(user.RecoveryMuscle))
                    .Take(1)
                    .Select(i => new ExerciseViewModel(user, i.Variation, IntensityLevel.WarmupCooldown, ExerciseActivityLevel.Warmup)
                    {
                        Demo = demo
                    })
                    .Concat(allExercises
                        // Make sure this is a recovery-level exercise // I don't know if I need this with exercise and intensity progressions.
                        //.Where(e => e.Variation.Intensities.Any(ip => ip.IntensityLevel == IntensityLevel.Recovery))
                        // Make sure this is a strengthening exercise
                        .Where(vm => vm.Variation.ExerciseType.HasFlag(ExerciseType.Strength))
                        // Choose recovery exercises that work the recovery muscle
                        .Where(i => i.Variation.Exercise.PrimaryMuscles.HasFlag(user.RecoveryMuscle))
                        .Take(1)
                        .Select(i => new ExerciseViewModel(user, i.Variation, IntensityLevel.Recovery, ExerciseActivityLevel.Main)
                        {
                            Demo = demo,
                        }))
                    .Concat(allExercises
                        // Make sure the exercise is a cooldown stretch
                        .Where(vm => vm.Variation.ExerciseType.HasFlag(ExerciseType.Flexibility))
                        // Choose dynamic stretches for warmups
                        .Where(e => e.Variation.MuscleContractions.HasFlag(MuscleContractions.Isometric))
                        // Choose recovery exercises that work the recovery muscle
                        .Where(i => i.Variation.Exercise.PrimaryMuscles.HasFlag(user.RecoveryMuscle))
                        .Take(1)
                        .Select(i => new ExerciseViewModel(user, i.Variation, IntensityLevel.WarmupCooldown, ExerciseActivityLevel.Cooldown)
                        {
                            Demo = demo
                        }))
                    .ToList();
            }

            // Sports exercises
            if (user.SportsFocus != SportsFocus.None && !newsletter.IsDeloadWeek)
            {
                var enduranceIntensityLevel = viewModel.ExerciseType == ExerciseType.Cardio ? IntensityLevel.Endurance : IntensityLevel.Gain;
                viewModel.SportsExercises = allExercises
                    // Make sure the exercise is for the correct workout type
                    .Where(vm => vm.Variation.ExerciseType.HasFlag(viewModel.ExerciseType))
                    // Choose recovery exercises that work the sports muscle
                    .Where(i => i.Variation.SportsFocus.HasFlag(user.SportsFocus))
                    .Take(3)
                    // Show most complex exercises first
                    .OrderByDescending(e => BitOperations.PopCount((ulong)e.Variation.Exercise.PrimaryMuscles))
                    .Select(i => new ExerciseViewModel(user, i.Variation, enduranceIntensityLevel, ExerciseActivityLevel.Main)
                    {
                        Demo = demo,
                    })
                    .ToList();
            }

            // Cooldown exercises
            var cooldownExercises = allExercises
                // Make sure the exercise is a cooldown stretch
                .Where(vm => vm.Variation.ExerciseType.HasFlag(ExerciseType.Flexibility))
                // Choose static stretches for cooldowns
                .Where(e => e.Variation.MuscleContractions.HasFlag(MuscleContractions.Isometric))
                // If a recovery muscle is set, don't choose any exercises that work the injured muscle
                .Where(i => user.RecoveryMuscle == MuscleGroups.None || !i.Variation.Exercise.AllMuscles.HasFlag(user.RecoveryMuscle))
                .Select(i => new ExerciseViewModel(user, i.Variation, IntensityLevel.WarmupCooldown, ExerciseActivityLevel.Cooldown)
                {
                    Demo = demo
                });
            viewModel.CooldownExercises = cooldownExercises
                .Aggregate(new List<ExerciseViewModel>(), (vms, vm) => (
                    // Grab compound exercises that cover at least two muscles in the targeted muscles set
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
