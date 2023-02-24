using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Web.Data.Query;
using Web.Entities.User;
using Web.Models.Exercise;
using Web.Models.Newsletter;
using Web.ViewModels.Exercise;
using Web.ViewModels.Newsletter;

namespace Web.Controllers.Newsletter;

public partial class NewsletterController
{
    /// <summary>
    /// Grab x-many exercises that the user hasn't seen in a long time.
    /// </summary>
    private async Task<List<ExerciseViewModel>> GetDebugExercises(Entities.User.User user, string token, int count = 1)
    {
        var baseQuery = _context.ExerciseVariations
            .AsNoTracking()
            .Include(v => v.Exercise)
                .ThenInclude(e => e.Prerequisites)
                    .ThenInclude(p => p.PrerequisiteExercise)
            .Include(ev => ev.Variation)
                .ThenInclude(i => i.Intensities)
            .Include(ev => ev.Variation)
                .ThenInclude(i => i.DefaultInstruction)
            .Include(v => v.Variation)
                .ThenInclude(i => i.Instructions.Where(eg => eg.Parent == null))
                    // To display the equipment required for the exercise in the newsletter
                    .ThenInclude(eg => eg.Equipment.Where(e => e.DisabledReason == null))
            .Include(v => v.Variation)
                .ThenInclude(i => i.Instructions.Where(eg => eg.Parent == null))
                    .ThenInclude(eg => eg.Children)
                        // To display the equipment required for the exercise in the newsletter
                        .ThenInclude(eg => eg.Equipment.Where(e => e.DisabledReason == null))
            .Select(a => new
            {
                ExerciseVariation = a,
                a.Variation,
                a.Exercise,
                UserExercise = a.Exercise.UserExercises.FirstOrDefault(uv => uv.User == user),
                UserExerciseVariation = a.UserExerciseVariations.FirstOrDefault(uv => uv.User == user),
                UserVariation = a.Variation.UserVariations.FirstOrDefault(uv => uv.User == user)
            });

        return (await baseQuery.ToListAsync())
            .GroupBy(i => new { i.Exercise.Id, LastSeen = i.UserExercise?.LastSeen ?? DateOnly.MinValue })
            .OrderBy(a => a.Key.LastSeen)
                .ThenBy(a => Guid.NewGuid())
            .Take(count)
            .SelectMany(e => e)
            .OrderBy(vm => vm.ExerciseVariation.Progression.Min)
                .ThenBy(vm => vm.ExerciseVariation.Progression.Max == null)
                .ThenBy(vm => vm.ExerciseVariation.Progression.Max)
            .Select(r => new ExerciseViewModel(user, r.Exercise, r.Variation, r.ExerciseVariation,
                r.UserExercise, r.UserExerciseVariation, r.UserVariation,
                easierVariation: null, harderVariation: null,
                intensityLevel: null, Theme: ExerciseTheme.Extra, token: token))
            .ToList();
    }

    [Route("debug")]
    public async Task<IActionResult> DebugNewsletter(string email, string token)
    {
        const string DebugUser = "debug@livetest.finerfettle.com";

        if (email != DebugUser)
        {
            return NoContent();
        }

        // The debug user is disabled, not checking that or rest days.
        var user = await _userService.GetUser(email, token, includeUserEquipments: true, includeVariations: true);

        // User was already sent a newsletter today
        if (user == null || await _context.Newsletters.Where(n => n.User == user).AnyAsync(n => n.Date == Today))
        {
            return NoContent();
        }

        // The exercise queryer requires UserExercise/UserExerciseVariation/UserVariation records to have already been made
        _context.AddMissing(await _context.UserExercises.Where(ue => ue.User == user).Select(ue => ue.ExerciseId).ToListAsync(),
            await _context.Exercises.Select(e => new { e.Id, e.Proficiency }).ToListAsync(), k => k.Id, e => new UserExercise() { ExerciseId = e.Id, UserId = user.Id, Progression = user.IsNewToFitness ? UserExercise.MinUserProgression : e.Proficiency });

        _context.AddMissing(await _context.UserExerciseVariations.Where(ue => ue.User == user).Select(uev => uev.ExerciseVariationId).ToListAsync(),
            await _context.ExerciseVariations.Select(ev => ev.Id).ToListAsync(), evId => new UserExerciseVariation() { ExerciseVariationId = evId, UserId = user.Id });

        _context.AddMissing(await _context.UserVariations.Where(ue => ue.User == user).Select(uv => uv.VariationId).ToListAsync(),
            await _context.Variations.Select(v => v.Id).ToListAsync(), vId => new UserVariation() { VariationId = vId, UserId = user.Id });

        await _context.SaveChangesAsync();

        user.EmailVerbosity = Verbosity.Debug;
        IList<ExerciseViewModel> debugExercises = await GetDebugExercises(user, token, count: 1);

        var newsletter = new Entities.Newsletter.Newsletter(Today, user, new NewsletterTypeGroups(user.Frequency).First(), false);
        var equipmentViewModel = new EquipmentViewModel(_context.Equipment.Where(e => e.DisabledReason == null), user.UserEquipments.Select(eu => eu.Equipment));
        var viewModel = new DebugViewModel(user, token)
        {
            AllEquipment = equipmentViewModel,
            DebugExercises = debugExercises,
        };

        await UpdateLastSeenDate(user, debugExercises, Enumerable.Empty<ExerciseViewModel>());

        ViewData[ViewData_Deload] = false;
        return View(nameof(DebugNewsletter), viewModel);
    }

    [Route("check")]
    public async Task<IActionResult> Check()
    {
        var allExercises = (await new QueryBuilder(_context, ignoreGlobalQueryFilters: true)
            .WithMuscleGroups(MuscleGroups.All, x =>
            {
                x.MuscleTarget = vm => vm.Variation.StabilityMuscles | vm.Variation.StretchMuscles | vm.Variation.StrengthMuscles;
            })
        .Build()
            .Query())
            .Select(r => new ExerciseViewModel(r, ExerciseTheme.Main))
            .ToList();

        var strengthExercises = (await new QueryBuilder(_context, ignoreGlobalQueryFilters: false)
            .WithMuscleGroups(MuscleGroups.All, x =>
            {
                x.MuscleTarget = vm => vm.Variation.StabilityMuscles | vm.Variation.StretchMuscles | vm.Variation.StrengthMuscles;
            })
            .WithRecoveryMuscle(MuscleGroups.None)
        .WithExerciseType(ExerciseType.Main)
        .Build()
            .Query())
            .Select(r => new ExerciseViewModel(r, ExerciseTheme.Main))
            .ToList();

        var recoveryExercises = (await new QueryBuilder(_context, ignoreGlobalQueryFilters: false)
            .WithMuscleGroups(MuscleGroups.All, x =>
            {
                x.MuscleTarget = vm => vm.Variation.StabilityMuscles | vm.Variation.StretchMuscles | vm.Variation.StrengthMuscles;
            })
        .WithRecoveryMuscle(MuscleGroups.All)
        .Build()
        .Query())
            .Select(r => new ExerciseViewModel(r, ExerciseTheme.Main))
            .ToList();

        var warmupCooldownExercises = (await new QueryBuilder(_context, ignoreGlobalQueryFilters: false)
            .WithMuscleGroups(MuscleGroups.All, x =>
            {
                x.MuscleTarget = vm => vm.Variation.StabilityMuscles | vm.Variation.StretchMuscles | vm.Variation.StrengthMuscles;
            })
            .WithExerciseType(ExerciseType.WarmupCooldown)
            .WithOnlyWeights(false)
            .WithProficency(x =>
            {
                x.DoCapAtProficiency = true;
            })
            .Build()
            .Query())
            .Select(r => new ExerciseViewModel(r, ExerciseTheme.Main))
        .ToList();

        var missingExercises = _context.Variations
            .IgnoreQueryFilters()
            .Where(v => v.DisabledReason == null)
            // Left outer join
            .GroupJoin(_context.ExerciseVariations,
                o => o.Id,
                i => i.Variation.Id,
                (o, i) => new { Variation = o, ExerciseVariations = i })
            .SelectMany(
                oi => oi.ExerciseVariations.DefaultIfEmpty(),
                (o, i) => new { o.Variation, ExerciseVariation = i })
            .Where(v => v.ExerciseVariation == null)
            .Select(v => v.Variation.Name)
            .ToList();

        var progressionRange = Enumerable.Range(UserExercise.MinUserProgression, UserExercise.MaxUserProgression - UserExercise.MinUserProgression);
        var missing100ProgressionRange = allExercises
            .Where(e => e.Variation.DisabledReason == null && e.ExerciseVariation.DisabledReason == null)
            .GroupBy(e => e.Exercise.Name)
            .Where(g => !progressionRange.All(p => g.Any(e => p >= e.ExerciseVariation.Progression.GetMinOrDefault && p < e.ExerciseVariation.Progression.GetMaxOrDefault)))
            .Select(e => e.Key)
            .ToList();

        var emptyDisabledString = allExercises
            .Where(e => e.Exercise.DisabledReason == string.Empty || e.Variation.DisabledReason == string.Empty)
            .Select(e => e.Exercise.Name)
            .ToList();

        // The secondary muscles of a stretch are too hard to nail down...
        var stretchHasStability = warmupCooldownExercises
            .Where(e => e.Variation.MuscleMovement != MuscleMovement.Plyometric)
            .Where(e => e.Variation.StabilityMuscles != MuscleGroups.None)
            .Select(e => e.Variation.Name)
            .ToList();

        var missingRepRange = allExercises
            .Where(e => e.Variation.Intensities.Any(p => p.Proficiency.MinReps != null && p.Proficiency.MaxReps == null || p.Proficiency.MinReps == null && p.Proficiency.MaxReps != null))
            .Select(e => e.Variation.Name)
            .ToList();

        var strengthIntensities = new List<IntensityLevel>() {
            IntensityLevel.Endurance,
            IntensityLevel.Hypertrophy,
            IntensityLevel.Strength,
            IntensityLevel.Stabilization
        };
        var missingProficiencyStrength = strengthExercises
            .Where(e => e.Variation.Intensities
                .IntersectBy(strengthIntensities, i => i.IntensityLevel)
                .Count() < strengthIntensities.Count
            )
            .Select(e => e.Variation.Name)
            .ToList();

        var missingProficiencyRecovery = recoveryExercises
            .Where(e => e.Variation.Intensities.All(p =>
                p.IntensityLevel != IntensityLevel.Recovery
            ))
            .Select(e => e.Variation.Name)
            .ToList();

        var warmupCooldownIntensities = new List<IntensityLevel>() {
            IntensityLevel.Warmup,
            IntensityLevel.Cooldown
        };
        var missingProficiencyWarmupCooldown = warmupCooldownExercises
            .Where(e => e.Variation.Intensities
                .IntersectBy(warmupCooldownIntensities, i => i.IntensityLevel)
                .Count() < warmupCooldownIntensities.Count
            )
            .Select(e => e.Variation.Name)
            .ToList();

        var viewModel = new CheckViewModel()
        {
            StretchHasStability = stretchHasStability,
            Missing100PProgressionRange = missing100ProgressionRange,
            MissingProficiencyStrength = missingProficiencyStrength,
            MissingProficiencyRecovery = missingProficiencyRecovery,
            MissingProficiencyWarmupCooldown = missingProficiencyWarmupCooldown,
            MissingRepRange = missingRepRange,
            EmptyDisabledString = emptyDisabledString,
            MissingExercises = missingExercises
        };

        return View(viewModel);
    }
}
