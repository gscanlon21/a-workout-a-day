using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Native.Models;

public class NewsletterModel
{
    public User User { get; set; }
    public Newsletter Newsletter { get; set; }
    public int Verbosity { get; set; }
    public List<ExerciseModel> MainExercises { get; set; }
    public List<ExerciseModel> PrehabExercises { get; set; }
    public List<ExerciseModel> RehabExercises { get; set; }
    public List<ExerciseModel> WarmupExercises { get; set; }
    public List<ExerciseModel> SportsExercises { get; set; }
    public List<ExerciseModel> CooldownExercises { get; set; }
}

public class ExerciseModel
{
    public int Theme { get; set; }
    public int IntensityLevel { get; set; }
    public Exercise Exercise { get; set; }
    public Variation Variation { get; set; }
    public ExerciseVariation ExerciseVariation { get; set; }
    public User User { get; set; }
    public UserExercise UserExercise { get; set; }
    public UserExerciseVariation UserExerciseVariation { get; set; }
    public UserVariation UserVariation { get; set; }
    public bool UserFirstTimeViewing { get; set; }
    public object EasierVariation { get; set; }
    public string HarderVariation { get; set; }
    public object EasierReason { get; set; }
    public object HarderReason { get; set; }
    public bool HasLowerProgressionVariation { get; set; }
    public bool HasHigherProgressionVariation { get; set; }
    public bool UserMinProgressionInRange { get; set; }
    public bool UserMaxProgressionInRange { get; set; }
    public bool UserProgressionInRange { get; set; }
    public List<Proficiency> Proficiencies { get; set; }
    public int Verbosity { get; set; }
    public bool Demo { get; set; }
    public bool InEmailContext { get; set; }
}


public class DefaultInstruction
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Link { get; set; }
    public object DisabledReason { get; set; }
}

public class Exercise
{
    public int Id { get; set; }
    public string Name { get; set; }
    public int Proficiency { get; set; }
    public int Groups { get; set; }
    public object Notes { get; set; }
    public object DisabledReason { get; set; }
}

public class ExerciseVariation
{
    public int Id { get; set; }
    public Progression Progression { get; set; }
    public int ExerciseType { get; set; }
    public int SportsFocus { get; set; }
    public object DisabledReason { get; set; }
    public object Notes { get; set; }
    public int ExerciseId { get; set; }
    public int VariationId { get; set; }
}

public class Instruction
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Link { get; set; }
    public object DisabledReason { get; set; }
}

public class Intensity
{
    public int Id { get; set; }
    public object DisabledReason { get; set; }
    public Proficiency Proficiency { get; set; }
    public Variation Variation { get; set; }
    public int IntensityLevel { get; set; }
}


public class Newsletter
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string Date { get; set; }
    public NewsletterRotation NewsletterRotation { get; set; }
    public int Frequency { get; set; }
    public bool IsDeloadWeek { get; set; }
}

public class NewsletterRotation
{
    public int Id { get; set; }
    public int MuscleGroups { get; set; }
    public int MovementPatterns { get; set; }
    public bool IsFullBody { get; set; }
}

public class Proficiency
{
    public bool Demo { get; set; }
    public Intensity Intensity { get; set; }
    public User User { get; set; }
    public UserVariation UserVariation { get; set; }
    public bool ShowName { get; set; }
    public bool FirstTimeViewing { get; set; }
}

public class Proficiency2
{
    public int? MinSecs { get; set; }
    public int? MaxSecs { get; set; }
    public int? MinReps { get; set; }
    public int? MaxReps { get; set; }
    public int Sets { get; set; }
    public int Volume { get; set; }
}

public class Progression
{
    public int? Min { get; set; }
    public int? Max { get; set; }
    public int MinOrDefault { get; set; }
    public int MaxOrDefault { get; set; }
}


public class User
{
    public bool Demo { get; set; }
    public string TimeUntilDeload { get; set; }
    public int Id { get; set; }
    public string Email { get; set; }
    public string Token { get; set; }
    public int Features { get; set; }
    public int FootnoteType { get; set; }
    public bool ShowStaticImages { get; set; }
    public bool SendMobilityWorkouts { get; set; }
    public string LastActive { get; set; }
    public int MobilityMuscles { get; set; }
    public bool IsNewToFitness { get; set; }
    public int SendDays { get; set; }
    public int PrehabFocus { get; set; }
    public int RehabFocus { get; set; }
    public int SportsFocus { get; set; }
    public int EmailVerbosity { get; set; }
    public int IntensityLevel { get; set; }
    public int Frequency { get; set; }
    public int RefreshFunctionalEveryXWeeks { get; set; }
    public int RefreshAccessoryEveryXWeeks { get; set; }
    public List<UserEquipment> UserEquipments { get; set; }
    public List<int> EquipmentIds { get; set; }
    public bool IsAlmostInactive { get; set; }
}

public class UserEquipment
{
    public int EquipmentId { get; set; }
    public int UserId { get; set; }
}

public class UserExercise
{
    public int UserId { get; set; }
    public int ExerciseId { get; set; }
    public int Progression { get; set; }
    public bool Ignore { get; set; }
    public string LastSeen { get; set; }
    public object RefreshAfter { get; set; }
}

public class UserExerciseVariation
{
    public int UserId { get; set; }
    public int ExerciseVariationId { get; set; }
    public string LastSeen { get; set; }
    public object RefreshAfter { get; set; }
}

public class UserVariation
{
    public int UserId { get; set; }
    public int VariationId { get; set; }
    public bool Ignore { get; set; }
    public int Pounds { get; set; }
}

public class Variation
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string StaticImage { get; set; }
    public string AnimatedImage { get; set; }
    public bool Unilateral { get; set; }
    public bool UseCaution { get; set; }
    public bool AntiGravity { get; set; }
    public bool IsWeighted { get; set; }
    public int MuscleContractions { get; set; }
    public int MuscleMovement { get; set; }
    public int MovementPattern { get; set; }
    public int ExerciseFocus { get; set; }
    public int MobilityJoints { get; set; }
    public int StrengthMuscles { get; set; }
    public int StretchMuscles { get; set; }
    public int SecondaryMuscles { get; set; }
    public object DisabledReason { get; set; }
    public object Notes { get; set; }
    public int AllMuscles { get; set; }
    public int? DefaultInstructionId { get; set; }
    public DefaultInstruction DefaultInstruction { get; set; }
    public List<Instruction> Instructions { get; set; }
}
