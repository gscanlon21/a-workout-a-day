using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FinerFettle.Web.Data;
using FinerFettle.Web.Models.User;
using FinerFettle.Web.Models.Exercise;
using FinerFettle.Web.ViewModels.Newsletter;
using FinerFettle.Web.Models.Newsletter;

namespace FinerFettle.Web.Controllers
{
    public class NewsletterController : Controller
    {
        private readonly CoreContext _context;

        /// <summary>
        /// The name of the controller for routing purposes
        /// </summary>
        public const string Name = "Newsletter";

        /// <summary>
        /// Today's date from UTC
        /// </summary>
        private static DateOnly Today => DateOnly.FromDateTime(DateTime.UtcNow);

        public NewsletterController(CoreContext context)
        {
            _context = context;
        }

        #region Helpers

        /// <summary>
        /// Grabs a user from an email address.
        /// </summary>
        private async Task<User> GetUser(string email)
        {
            return await _context.Users
                // For displaying ignored exercises in the bottom of the newsletter
                .Include(u => u.UserExercises)
                    .ThenInclude(ep => ep.Exercise)
                // For displaying user's equipment in the bottom of the newsletter
                .Include(u => u.UserEquipments)
                    .ThenInclude(u => u.Equipment)
                .FirstAsync(u => u.Email == email);
        }

        /// <summary>
        /// Grabs the previous newsletter received by the user.
        /// </summary>
        private async Task<Newsletter?> GetPreviousNewsletter(User user)
        {
            return await _context.Newsletters
                .Where(n => n.User == user)
                .OrderBy(n => n.Date)
                .ThenBy(n => n.Id) // For testing/demo. When two newsletters get sent in the same day, I want a different exercise set.
                .LastOrDefaultAsync();
        }

        /// <summary>
        /// Calculates the user's next newsletter type (strength/stability/cardio) from the previous newsletter.
        /// </summary>
        private static ExerciseRotation GetTodoExerciseType(User user, Newsletter? previousNewsletter)
        {
            var todoExerciseType = new ExerciseTypeGroups(user.StrengtheningPreference).First(); // Have to start somewhere
            if (previousNewsletter != null)
            {
                todoExerciseType = new ExerciseTypeGroups(user.StrengtheningPreference)
                    .SkipWhile(r => r != previousNewsletter.ExerciseRotation)
                    .Skip(1)
                    .FirstOrDefault() ?? todoExerciseType;
            }
            return todoExerciseType;
        }

        /// <summary>
        /// Checks if the user should deload for a week (reduce the intensity of their workout to reduce muscle growth stagnating).
        /// </summary>
        private async Task<bool> CheckNewsletterDeloadStatus(User user)
        {
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
                    (lastDeload.IsDeloadWeek && lastDeload.Date.AddDays(-1 * (int)lastDeload.Date.DayOfWeek) == Today.AddDays(-1 * (int)Today.DayOfWeek))
                    // Or the last deload/oldest newsletter was 1+ months ago
                    || lastDeload.Date.AddMonths(1) < Today
                );

            return needsDeload;
        }

        /// <summary>
        /// Creates a new instance of the newsletter and saves it.
        /// </summary>
        private async Task<Newsletter> CreateAndAddNewsletterToContext(User user, ExerciseRotation todoExerciseType, bool needsDeload)
        {
            var newsletter = new Newsletter()
            {
                IsDeloadWeek = needsDeload,
                Date = Today,
                User = user,
                ExerciseRotation = todoExerciseType
            };
            _context.Newsletters.Add(newsletter);
            await _context.SaveChangesAsync();
            return newsletter;
        }

        #endregion

        [Route("newsletter/{email}")]
        public async Task<IActionResult> Newsletter(string email, bool demo = false)
        {
            var user = await GetUser(email);
            if (user.Disabled || user.RestDays.HasFlag(RestDaysExtensions.FromDate(Today)))
            {
                return NoContent();
            }

            var previousNewsletter = await GetPreviousNewsletter(user);

            var todoExerciseType = GetTodoExerciseType(user, previousNewsletter);
            var needsDeload = await CheckNewsletterDeloadStatus(user);

            var newsletter = await CreateAndAddNewsletterToContext(user, todoExerciseType, needsDeload);

            var mainExercises = new ExerciseQueryBuilder(_context, user, demo)
                .WithExerciseType(todoExerciseType.ExerciseType)
                .WithMuscleGroups(todoExerciseType.MuscleGroups)
                .WithRecoveryMuscle(user.RecoveryMuscle)
                .WithActivityLevel(ExerciseActivityLevel.Main)
                .WithPrefersWeights(user.PrefersWeights ? true : null)
                .CapAtProficiency(needsDeload)
                .WithAtLeastXUniqueMusclesPerExercise(2)
                .Build();

            var warmupCardio = new ExerciseQueryBuilder(_context, user, demo)
                    .WithExerciseType(ExerciseType.Cardio)
                    .WithMuscleGroups(todoExerciseType.MuscleGroups)
                    .WithIntensityLevel(IntensityLevel.WarmupCooldown)
                    .WithActivityLevel(ExerciseActivityLevel.Warmup)
                    .WithMuscleContractions(MuscleContractions.Dynamic)
                    .WithRecoveryMuscle(user.RecoveryMuscle)
                    .WithPrefersWeights(false)
                    .CapAtProficiency(true)
                    .Take(1)
                    .Build();
            var warmupExercises = new ExerciseQueryBuilder(_context, user, demo)
                .WithExerciseType(todoExerciseType.ExerciseType == ExerciseType.Cardio ? ExerciseType.Cardio : ExerciseType.Flexibility)
                .WithMuscleGroups(todoExerciseType.MuscleGroups/*.UnsetFlag32(warmupCardio.Aggregate((MuscleGroups)0, (acc, next) => acc | next.Exercise.PrimaryMuscles))*/)
                .WithIntensityLevel(IntensityLevel.WarmupCooldown)
                .WithActivityLevel(ExerciseActivityLevel.Warmup)
                .WithMuscleContractions(MuscleContractions.Dynamic)
                .WithRecoveryMuscle(user.RecoveryMuscle)
                .WithPrefersWeights(false)
                .WithAtLeastXUniqueMusclesPerExercise(3)
                .CapAtProficiency(true)
                .Build()
                .UnionBy(warmupCardio, k => k.Variation.Id)
                .ToList();

            // Recovery exercises
            IList<ExerciseViewModel>? recoveryExercises = null;
            if (user.RecoveryMuscle != MuscleGroups.None)
            {
                recoveryExercises = new ExerciseQueryBuilder(_context, user, demo)
                    .WithExerciseType(ExerciseType.Flexibility)
                    .WithIntensityLevel(IntensityLevel.WarmupCooldown)
                    .WithActivityLevel(ExerciseActivityLevel.Warmup)
                    .WithMuscleContractions(MuscleContractions.Dynamic)
                    .WithRecoveryMuscle(user.RecoveryMuscle, include: true)
                    .WithPrefersWeights(false)
                    .CapAtProficiency(true)
                    .Take(1)
                    .Build()
                    .Concat(new ExerciseQueryBuilder(_context, user, demo)
                        .WithExerciseType(ExerciseType.Strength)
                        .WithIntensityLevel(IntensityLevel.Recovery)
                        .WithActivityLevel(ExerciseActivityLevel.Main)
                        .WithRecoveryMuscle(user.RecoveryMuscle, include: true)
                        .WithPrefersWeights(user.PrefersWeights ? true : null)
                        .Take(1)
                        .Build())
                    .Concat(new ExerciseQueryBuilder(_context, user, demo)
                        .WithExerciseType(ExerciseType.Flexibility)
                        .WithIntensityLevel(IntensityLevel.WarmupCooldown)
                        .WithActivityLevel(ExerciseActivityLevel.Cooldown)
                        .WithMuscleContractions(MuscleContractions.Isometric)
                        .WithRecoveryMuscle(user.RecoveryMuscle, include: true)
                        .WithPrefersWeights(false)
                        .CapAtProficiency(true)
                        .Take(1)
                        .Build())
                    .ToList();
            }

            // Sports exercises
            IList<ExerciseViewModel>? sportsExercises = null;
            if (user.SportsFocus != SportsFocus.None)
            {
                sportsExercises = new ExerciseQueryBuilder(_context, user, demo)
                    .WithExerciseType(todoExerciseType.ExerciseType)
                    .WithIntensityLevel(todoExerciseType.ExerciseType == ExerciseType.Cardio ? IntensityLevel.Endurance : IntensityLevel.Gain)
                    .WithActivityLevel(ExerciseActivityLevel.Main)
                    .WithMuscleContractions(MuscleContractions.Dynamic)
                    .WithSportsFocus(user.SportsFocus)
                    .WithRecoveryMuscle(user.RecoveryMuscle)
                    .CapAtProficiency(needsDeload)
                    .Take(3)
                    .Build();
            }

            var cooldownExercises = new ExerciseQueryBuilder(_context, user, demo)
                .WithExerciseType(ExerciseType.Flexibility)
                .WithMuscleGroups(todoExerciseType.MuscleGroups)
                .WithIntensityLevel(IntensityLevel.WarmupCooldown)
                .WithActivityLevel(ExerciseActivityLevel.Cooldown)
                .WithRecoveryMuscle(user.RecoveryMuscle)
                .WithMuscleContractions(MuscleContractions.Isometric)
                .WithPrefersWeights(false)
                .WithAtLeastXUniqueMusclesPerExercise(3)
                .CapAtProficiency(true)
                .Build();

            var equipmentViewModel = new EquipmentViewModel(_context.Equipment.Where(e => e.DisabledReason == null), user.UserEquipments.Select(eu => eu.Equipment));
            var viewModel = new NewsletterViewModel(mainExercises, user, newsletter)
            {
                ExerciseType = todoExerciseType.ExerciseType,
                MuscleGroups = todoExerciseType.MuscleGroups,
                AllEquipment = equipmentViewModel,
                SportsExercises = sportsExercises,
                RecoveryExercises = recoveryExercises,
                CooldownExercises = cooldownExercises,
                WarmupExercises = warmupExercises,
                Demo = demo
            };

            return View(nameof(Newsletter), viewModel);
        }
    }
}
