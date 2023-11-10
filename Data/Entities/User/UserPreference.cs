using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;
using System.Text.Json.Serialization;

namespace Data.Entities.User;

/// <summary>
/// User who signed up for the newsletter.
/// </summary>
[Table("user_preference"), Comment("Advanced workout settings")]
[DebuggerDisplay("Email = {Email}, LastActive = {LastActive}")]
public class UserPreference
{
    public class Consts
    {
        public const int AtLeastXUniqueMusclesPerExercise_FlexibilityMin = 1;
        public const int AtLeastXUniqueMusclesPerExercise_FlexibilityDefault = 2;
        public const int AtLeastXUniqueMusclesPerExercise_FlexibilityMax = 4;
        
        public const int AtLeastXUniqueMusclesPerExercise_MobilityMin = 1;
        public const int AtLeastXUniqueMusclesPerExercise_MobilityDefault = 2;
        public const int AtLeastXUniqueMusclesPerExercise_MobilityMax = 4;

        public const int AtLeastXUniqueMusclesPerExercise_AccessoryMin = 1;
        public const int AtLeastXUniqueMusclesPerExercise_AccessoryDefault = 2;
        public const int AtLeastXUniqueMusclesPerExercise_AccessoryMax = 4;

        public const double WeightIsolationXTimesMoreMin = 1;
        public const double WeightIsolationXTimesMoreDefault = 1.5;
        public const double WeightIsolationXTimesMoreMax = 2;

        public const double WeightSecondaryMusclesXTimesLessMin = 2;
        public const double WeightSecondaryMusclesXTimesLessDefault = 3;
        public const double WeightSecondaryMusclesXTimesLessMax = 4;
    }

    [Obsolete("Public parameterless constructor for model binding.", error: true)]
    public UserPreference() { }

    /// <summary>
    /// Creates a new user.
    /// </summary>
    public UserPreference(User user)
    {
        User = user;
        UserId = user.Id;
    }

    public int UserId { get; private init; }

    public bool IgnorePrerequisites { get; set; }

    public int AtLeastXUniqueMusclesPerExercise_Mobility { get; set; } = Consts.AtLeastXUniqueMusclesPerExercise_MobilityDefault;
    public int AtLeastXUniqueMusclesPerExercise_Flexibility { get; set; } = Consts.AtLeastXUniqueMusclesPerExercise_FlexibilityDefault;
    public int AtLeastXUniqueMusclesPerExercise_Accessory { get; set; } = Consts.AtLeastXUniqueMusclesPerExercise_AccessoryDefault;

    public double WeightSecondaryMusclesXTimesLess { get; set; } = Consts.WeightSecondaryMusclesXTimesLessDefault;
    public double WeightIsolationXTimesMore { get; set; } = Consts.WeightIsolationXTimesMoreDefault;

    #region Navigation Properties

    [JsonIgnore, InverseProperty(nameof(User.UserPreference))]
    public virtual User User { get; private init; } = null!;

    #endregion
}
