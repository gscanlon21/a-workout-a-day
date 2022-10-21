using FinerFettle.Web.Data;
using FinerFettle.Web.Models.Exercise;
using FinerFettle.Web.Models.Newsletter;
using FinerFettle.Web.Models.User;
using FinerFettle.Web.ViewModels.Exercise;
using FinerFettle.Web.ViewModels.Newsletter;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NuGet.Common;

namespace FinerFettle.Web.Controllers
{
    [Route("exercises")]
    public class ExerciseController : BaseController
    {
        /// <summary>
        /// The name of the controller for routing purposes
        /// </summary>
        public const string Name = "Exercise";

        public ExerciseController(CoreContext context) : base(context) { }

        [Route("all")]
        public async Task<IActionResult> All()
        {
            var allExercises = new ExerciseQueryBuilder(_context, user: null, demo: false)
                .WithMuscleGroups(MuscleGroups.All)
                .WithOrderBy(ExerciseQueryBuilder.OrderByEnum.Progression)
                .Build("");
         
            var viewModel = new ExercisesViewModel(allExercises, Verbosity.Debug);

            return View(viewModel);
        }

        [Route("check")]
        public async Task<IActionResult> Check()
        {
            var allExercises = new ExerciseQueryBuilder(_context, user: null, demo: false)
                .WithMuscleGroups(MuscleGroups.All)
                .Build("");

            var viewModel = new ExercisesViewModel(allExercises, Verbosity.Debug);

            return View(viewModel);
        }
    }
}
