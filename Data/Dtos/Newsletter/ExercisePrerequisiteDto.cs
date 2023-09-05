
using Data.Entities.Exercise;

namespace Data.Dtos.Newsletter;

public class ExercisePrerequisiteDto
{
    public ExercisePrerequisiteDto(ExercisePrerequisite exercisePrerequisite)
    {
        Proficiency = exercisePrerequisite.Proficiency;
        Id = exercisePrerequisite.PrerequisiteExerciseId;
        Name = exercisePrerequisite.PrerequisiteExercise.Name;
    }

    public int Proficiency { get; set; }
    public int Id { get; set; }
    public string Name { get; set; } = null!;
}
