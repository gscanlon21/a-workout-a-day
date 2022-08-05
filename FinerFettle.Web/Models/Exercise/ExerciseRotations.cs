using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FinerFettle.Web.Models.Exercise
{
    [Owned]
    public record ExerciseRotaion(int id, ExerciseType ExerciseType, MuscleGroups MuscleGroups);
}
