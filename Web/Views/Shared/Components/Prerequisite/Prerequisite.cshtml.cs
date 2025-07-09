﻿using Core.Dtos.Exercise;
using Core.Dtos.Newsletter;
using Core.Dtos.User;
using Core.Models.Newsletter;

namespace Web.Views.Shared.Components.Prerequisite;

public class PrerequisiteViewModel
{
    /// <summary>
    /// Showing the progression bar so the user can see where they are in the progression track.
    /// </summary>
    public Verbosity Verbosity => Verbosity.Instructions | Verbosity.ProgressionBar;

    public required bool Open { get; init; }
    public required UserNewsletterDto UserNewsletter { get; init; }
    public required IDictionary<int, int> ExerciseProficiencyMap { get; init; }
    public required IList<ExerciseVariationDto> VisiblePrerequisites { get; init; }
    public required IList<ExerciseVariationDto> InvisiblePrerequisites { get; init; }

    public class ExerciseSectionComparer : IEqualityComparer<ExerciseVariationDto>
    {
        public int GetHashCode(ExerciseVariationDto e) => e.Exercise.GetHashCode();
        public bool Equals(ExerciseVariationDto? a, ExerciseVariationDto? b)
            => EqualityComparer<ExerciseDto>.Default.Equals(a?.Exercise, b?.Exercise);
    }
}
