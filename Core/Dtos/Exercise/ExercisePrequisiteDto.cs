using Core.Consts;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;

namespace Core.Dtos.Exercise;

/// <summary>
/// Pre-requisite exercises for other exercises.
/// </summary>
[DebuggerDisplay("Id = {Id}, Name = {Name,nq}, Proficiency = {Proficiency}")]
public class ExercisePrerequisiteDto
{
    /// <summary>
    /// The Id of the prerequisite/postrequisite exercise.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// The Name of the prerequisite/postrequisite exercise.
    /// </summary>
    public string Name { get; set; } = null!;

    /// <summary>
    /// The progression level of the prerequisite the user needs to be at to unlock the postrequisite.
    /// </summary>
    [Required, Range(UserConsts.MinUserProgression, UserConsts.MaxUserProgression)]
    public int Proficiency { get; init; }
}
