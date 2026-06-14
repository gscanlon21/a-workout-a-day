using Core.Models.User;
using Data;
using Data.Entities.Exercises;
using Data.Repos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Web.Code.TempData;
using Web.Controllers.Users;
using static System.Net.WebRequestMethods;

namespace Web.Controllers.Exercises;

[Route($"{Name}")]
public class UpsertExerciseVariationController : ViewController
{
    /// <summary>
    /// The name of the controller for routing purposes.
    /// </summary>
    public const string Name = "UpsertExerciseVariation";

    private readonly CoreContext _context;
    private readonly UserRepo _userRepo;

    public UpsertExerciseVariationController(CoreContext context, UserRepo userRepo)
    {
        _context = context;
        _userRepo = userRepo;
    }

    [Route(""), AcceptVerbs(Http.Post)]
    public async Task<IActionResult> UpsertExercise(Exercise exercise, string email, string token)
    {
        var user = await _userRepo.GetUserStrict(email, token, allowDemoUser: false);
        if (!user.Features.HasFlag(Features.Admin))
        {
            return View("StatusMessage", new StatusMessageViewModel(LinkExpiredMessage));
        }

        var existingEntity = await _context.Exercises.FirstOrDefaultAsync(r => r.Id == exercise.Id);
        if (existingEntity != null)
        {
            await _context.Exercises.Where(e => e.Id == exercise.Id).ExecuteUpdateAsync(ef => ef
                .SetProperty(e => e.Name, exercise.Name)
                .SetProperty(e => e.Notes, exercise.Notes)
                .SetProperty(e => e.VocalSkills, exercise.VocalSkills)
                .SetProperty(e => e.VisualSkills, exercise.VisualSkills)
                .SetProperty(e => e.LumbarSkills, exercise.LumbarSkills)
                .SetProperty(e => e.ThoracicSkills, exercise.ThoracicSkills)
                .SetProperty(e => e.CervicalSkills, exercise.CervicalSkills)
                .SetProperty(e => e.StartingProgression, exercise.StartingProgression)
                .SetProperty(e => e.DisabledReason, exercise.DisabledReason)
            );
        }
        else
        {
            throw new NotImplementedException();
        }

        TempData[TempData_User.SuccessMessage] = "Your exercise has been updated!";
        return RedirectToAction(nameof(UserController.ManageExerciseVariation), new { email, token, exercise.Id, wasUpdated = true });
    }

    [Route(""), AcceptVerbs(Http.Post)]
    public async Task<IActionResult> UpsertVariation(Variation variation, string email, string token)
    {
        var user = await _userRepo.GetUserStrict(email, token, allowDemoUser: false);
        if (!user.Features.HasFlag(Features.Admin))
        {
            return View("StatusMessage", new StatusMessageViewModel(LinkExpiredMessage));
        }

        var existingEntity = await _context.Variations.FirstOrDefaultAsync(r => r.Id == variation.Id);
        if (existingEntity != null)
        {
            await _context.Variations.Where(v => v.Id == variation.Id).ExecuteUpdateAsync(ef => ef
                .SetProperty(v => v.Name, variation.Name)
                .SetProperty(v => v.Notes, variation.Notes)
                .SetProperty(v => v.AnimatedImage, variation.AnimatedImage)
                .SetProperty(v => v.IsWeighted, variation.IsWeighted)
                .SetProperty(v => v.MovementPattern, variation.MovementPattern)
                .SetProperty(v => v.MuscleMovement, variation.MuscleMovement)
                .SetProperty(v => v.Stabilizes, variation.Stabilizes)
                .SetProperty(v => v.StaticImage, variation.StaticImage)
                .SetProperty(v => v.Strengthens, variation.Strengthens)
                .SetProperty(v => v.Stretches, variation.Stretches)
                .SetProperty(v => v.Unilateral, variation.Unilateral)
                .SetProperty(v => v.UseCaution, variation.UseCaution)
                .SetProperty(v => v.ExerciseFocus, variation.ExerciseFocus)
                .SetProperty(v => v.SportsFocus, variation.SportsFocus)
                .SetProperty(v => v.PauseReps, variation.PauseReps)
                .SetProperty(v => v.DefaultInstruction, variation.DefaultInstruction)
                .SetProperty(v => v.Progression.Max, variation.Progression.Max)
                .SetProperty(v => v.Progression.Min, variation.Progression.Min)
                .SetProperty(v => v.Section, variation.Section)
                .SetProperty(v => v.DisabledReason, variation.DisabledReason)
            );
        }
        else
        {
            throw new NotImplementedException();
        }

        TempData[TempData_User.SuccessMessage] = "Your variation has been updated!";
        return RedirectToAction(nameof(UserController.ManageExerciseVariation), new { email, token, variation.Id, wasUpdated = true });
    }
}
