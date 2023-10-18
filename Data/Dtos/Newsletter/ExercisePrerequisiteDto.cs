
using Data.Entities.Exercise;

namespace Data.Dtos.Newsletter;

public class ExercisePrerequisiteDto(ExercisePrerequisite exercisePrerequisite)
{
    public int Proficiency { get; set; } = exercisePrerequisite.Proficiency;
    public int Id { get; set; } = exercisePrerequisite.PrerequisiteExerciseId;
    public string Name { get; set; } = exercisePrerequisite.PrerequisiteExercise.Name;
}
