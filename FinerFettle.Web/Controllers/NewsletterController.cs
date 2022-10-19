using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FinerFettle.Web.Data;
using FinerFettle.Web.Models.User;
using FinerFettle.Web.Models.Exercise;
using FinerFettle.Web.ViewModels.Newsletter;
using FinerFettle.Web.Models.Newsletter;
using FinerFettle.Web.Extensions;

namespace FinerFettle.Web.Controllers
{
    public class NewsletterController : BaseController
    {
        /// <summary>
        /// The name of the controller for routing purposes
        /// </summary>
        public const string Name = "Newsletter";

        /// <summary>
        /// Today's date from UTC
        /// </summary>
        private static DateOnly Today => DateOnly.FromDateTime(DateTime.UtcNow);

        public NewsletterController(CoreContext context) : base(context) { }

        #region Helpers

        /// <summary>
        /// Grabs a user from an email address.
        /// </summary>
        private async Task<User> GetUser(string email, string token)
        {
            return await _context.Users
                // For displaying ignored exercises in the bottom of the newsletter
                .Include(u => u.UserExercises)
                    .ThenInclude(ep => ep.Exercise)
                // For displaying user's equipment in the bottom of the newsletter
                .Include(u => u.UserEquipments) // Has a global query filter to filter out disabled equipment
                    .ThenInclude(u => u.Equipment)
                .FirstAsync(u => u.Email == email && (u.UserTokens.Any(ut => ut.Token == token) || email == Models.User.User.DemoUser));
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
        private static NewsletterRotation GetTodaysNewsletterRotation(User user, Newsletter? previousNewsletter)
        {
            var todaysNewsletterRotation = new NewsletterTypeGroups(user.StrengtheningPreference).First(); // Have to start somewhere
            if (previousNewsletter != null)
            {
                todaysNewsletterRotation = new NewsletterTypeGroups(user.StrengtheningPreference)
                    .SkipWhile(r => r != previousNewsletter.NewsletterRotation)
                    .Skip(1)
                    .FirstOrDefault() ?? todaysNewsletterRotation;
            }
            return todaysNewsletterRotation;
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
        private async Task<Newsletter> CreateAndAddNewsletterToContext(User user, NewsletterRotation todoNewsletterType, bool needsDeload)
        {
            var newsletter = new Newsletter()
            {
                IsDeloadWeek = needsDeload,
                Date = Today,
                User = user,
                NewsletterRotation = todoNewsletterType
            };
            _context.Newsletters.Add(newsletter);
            await _context.SaveChangesAsync();
            return newsletter;
        }

        /// <summary>
        /// User is receiving a new newsletter. Generate a new token for links.
        /// </summary>
        private async Task<string> SetAndSaveNewAuthToken(User user)
        {
            var token = new UserToken(user.Id) 
            {
                // Unsubscribe links need to work for at least 60 days per law
                Expires = DateOnly.FromDateTime(DateTime.UtcNow).AddMonths(3) 
            };
            user.UserTokens.Add(token);
            await _context.SaveChangesAsync();
            return token.Token;
        }

        #endregion

        [Route("newsletter/{email}")]
        public async Task<IActionResult> Newsletter(string email, string token, bool? demo = null)
        {
            var user = await GetUser(email, token);
            if (user.Disabled || user.RestDays.HasFlag(RestDaysExtensions.FromDate(Today)))
            {
                return NoContent();
            }

            var previousNewsletter = await GetPreviousNewsletter(user);

            // User has received an email with a confirmation message, but they did not click to confirm their account
            if (previousNewsletter != null && user.LastActive == null)
            {
                return NoContent();
            }

            token = await SetAndSaveNewAuthToken(user);

            var todaysNewsletterRotation = GetTodaysNewsletterRotation(user, previousNewsletter);
            var needsDeload = await CheckNewsletterDeloadStatus(user);

            var newsletter = await CreateAndAddNewsletterToContext(user, todaysNewsletterRotation, needsDeload);

            var mainExercises = new ExerciseQueryBuilder(_context, user, demo)
                .WithExerciseType(todaysNewsletterRotation.ExerciseType)
                .WithMuscleGroups(todaysNewsletterRotation.MuscleGroups)
                .WithIntensityLevel(todaysNewsletterRotation.IntensityLevel)
                .WithRecoveryMuscle(user.RecoveryMuscle)
                .WithActivityLevel(ExerciseActivityLevel.Main)
                .WithPrefersWeights(user.PrefersWeights ? true : null)
                .CapAtProficiency(needsDeload)
                .WithAtLeastXUniqueMusclesPerExercise(todaysNewsletterRotation.MuscleGroups == MuscleGroups.All ? 3 : 2)
                .Build(token);

            var warmupCardio = new ExerciseQueryBuilder(_context, user, demo)
                    .WithExerciseType(ExerciseType.Cardio)
                    .WithMuscleGroups(todaysNewsletterRotation.MuscleGroups)
                    .WithIntensityLevel(IntensityLevel.WarmupCooldown)
                    .WithActivityLevel(ExerciseActivityLevel.Warmup)
                    .WithMuscleContractions(MuscleContractions.Dynamic)
                    .WithRecoveryMuscle(user.RecoveryMuscle)
                    .WithPrefersWeights(false)
                    .CapAtProficiency(true)
                    .Take(1)
                    .Build(token);
            var warmupExercises = new ExerciseQueryBuilder(_context, user, demo)
                .WithExerciseType(ExerciseType.Strength | ExerciseType.Flexibility | ExerciseType.Stability)
                .WithMuscleGroups(todaysNewsletterRotation.MuscleGroups/*.UnsetFlag32(warmupCardio.Aggregate((MuscleGroups)0, (acc, next) => acc | next.Exercise.PrimaryMuscles))*/)
                .WithIntensityLevel(IntensityLevel.WarmupCooldown)
                .WithActivityLevel(ExerciseActivityLevel.Warmup)
                .WithMuscleContractions(MuscleContractions.Dynamic)
                .WithRecoveryMuscle(user.RecoveryMuscle)
                .WithPrefersWeights(false)
                .WithAtLeastXUniqueMusclesPerExercise(3)
                .CapAtProficiency(true)
                .Build(token)
                .UnionBy(warmupCardio, k => k.Variation.Id)
                .ToList();

            // Recovery exercises
            IList<ExerciseViewModel>? recoveryExercises = null;
            if (user.RecoveryMuscle != MuscleGroups.None)
            {
                // Should recoveru exercises target muscles in isolation?
                recoveryExercises = new ExerciseQueryBuilder(_context, user, demo)
                    .WithExerciseType(ExerciseType.Strength | ExerciseType.Flexibility | ExerciseType.Stability)
                    .WithIntensityLevel(IntensityLevel.WarmupCooldown)
                    .WithActivityLevel(ExerciseActivityLevel.Warmup)
                    .WithMuscleContractions(MuscleContractions.Dynamic)
                    .WithRecoveryMuscle(user.RecoveryMuscle, include: true)
                    .WithPrefersWeights(false)
                    .CapAtProficiency(true)
                    .Take(1)
                    .Build(token)
                    .Concat(new ExerciseQueryBuilder(_context, user, demo)
                        .WithExerciseType(ExerciseType.Strength)
                        .WithIntensityLevel(IntensityLevel.Recovery)
                        .WithActivityLevel(ExerciseActivityLevel.Main)
                        .WithRecoveryMuscle(user.RecoveryMuscle, include: true)
                        .WithPrefersWeights(user.PrefersWeights ? true : null)
                        .Take(1)
                        .Build(token))
                    .Concat(new ExerciseQueryBuilder(_context, user, demo)
                        .WithExerciseType(ExerciseType.Flexibility)
                        .WithIntensityLevel(IntensityLevel.WarmupCooldown)
                        .WithActivityLevel(ExerciseActivityLevel.Cooldown)
                        .WithMuscleContractions(MuscleContractions.Isometric)
                        .WithRecoveryMuscle(user.RecoveryMuscle, include: true)
                        .WithPrefersWeights(false)
                        .CapAtProficiency(true)
                        .Take(1)
                        .Build(token))
                    .ToList();
            }

            // Sports exercises
            IList<ExerciseViewModel>? sportsExercises = null;
            if (user.SportsFocus != SportsFocus.None)
            {
                sportsExercises = new ExerciseQueryBuilder(_context, user, demo)
                    .WithExerciseType(ExerciseType.Strength | ExerciseType.Cardio)
                    .WithIntensityLevel(todaysNewsletterRotation.IntensityLevel == IntensityLevel.Endurance ? IntensityLevel.Endurance : IntensityLevel.Gain)
                    .WithActivityLevel(ExerciseActivityLevel.Main)
                    .WithMuscleContractions(MuscleContractions.Dynamic)
                    .WithSportsFocus(user.SportsFocus)
                    .WithRecoveryMuscle(user.RecoveryMuscle)
                    .CapAtProficiency(needsDeload)
                    .Take(3)
                    .Build(token);
            }

            var cooldownExercises = new ExerciseQueryBuilder(_context, user, demo)
                .WithExerciseType(ExerciseType.Flexibility)
                .WithMuscleGroups(todaysNewsletterRotation.MuscleGroups)
                .WithIntensityLevel(IntensityLevel.WarmupCooldown)
                .WithActivityLevel(ExerciseActivityLevel.Cooldown)
                .WithRecoveryMuscle(user.RecoveryMuscle)
                .WithMuscleContractions(MuscleContractions.Isometric)
                .WithPrefersWeights(false)
                .WithAtLeastXUniqueMusclesPerExercise(3)
                .CapAtProficiency(true)
                .Build(token);

            var equipmentViewModel = new EquipmentViewModel(_context.Equipment.Where(e => e.DisabledReason == null), user.UserEquipments.Select(eu => eu.Equipment));
            var viewModel = new NewsletterViewModel(mainExercises, user, newsletter, token)
            {
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
