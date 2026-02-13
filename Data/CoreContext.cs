using Core.Models.Exercise;
using Data.Entities.Equipment;
using Data.Entities.Exercise;
using Data.Entities.Footnote;
using Data.Entities.Newsletter;
using Data.Entities.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.ComponentModel;
using System.Reflection;
using System.Text.Json;

namespace Data;

/// <summary>
/// https://mehdi.me/ambient-dbcontext-in-ef6/
/// </summary>
public class CoreContext : DbContext
{
    private static readonly JsonSerializerOptions JsonSerializerOptions = new();

    private const string DISABLED_REASON_IS_NULL = "\"DisabledReason\" IS NULL";

    [Obsolete("Public parameterless constructor required for EF Core.", error: true)]
    public CoreContext() : base() { }
    public CoreContext(DbContextOptions<CoreContext> context) : base(context) { }

    public DbSet<User> Users { get; set; } = null!;
    public DbSet<Exercise> Exercises { get; set; } = null!;
    public DbSet<Variation> Variations { get; set; } = null!;
    public DbSet<UserToken> UserTokens { get; set; } = null!;
    public DbSet<UserEmail> UserEmails { get; set; } = null!;
    public DbSet<UserWorkout> UserWorkouts { get; set; } = null!;
    public DbSet<UserFootnote> UserFootnotes { get; set; } = null!;
    public DbSet<UserExercise> UserExercises { get; set; } = null!;
    public DbSet<UserVariation> UserVariations { get; set; } = null!;
    public DbSet<UserFrequency> UserFrequencies { get; set; } = null!;
    public DbSet<UserPrehabSkill> UserPrehabSkills { get; set; } = null!;
    public DbSet<UserVariationLog> UserVariationLogs { get; set; } = null!;
    public DbSet<UserMuscleStrength> UserMuscleStrengths { get; set; } = null!;
    public DbSet<UserMuscleMobility> UserMuscleMobilities { get; set; } = null!;
    public DbSet<UserWorkoutVariation> UserWorkoutVariations { get; set; } = null!;
    public DbSet<ExercisePrerequisite> ExercisePrerequisites { get; set; } = null!;
    public DbSet<UserMuscleFlexibility> UserMuscleFlexibilities { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ////////// Keys //////////
        modelBuilder.Entity<UserFrequency>().HasKey(sc => new { sc.UserId, sc.Id });
        modelBuilder.Entity<UserPrehabSkill>().HasKey(sc => new { sc.UserId, sc.PrehabFocus });
        modelBuilder.Entity<UserMuscleStrength>().HasKey(sc => new { sc.UserId, sc.MuscleGroup });
        modelBuilder.Entity<UserMuscleMobility>().HasKey(sc => new { sc.UserId, sc.MuscleGroup });
        modelBuilder.Entity<UserMuscleFlexibility>().HasKey(sc => new { sc.UserId, sc.MuscleGroup });
        modelBuilder.Entity<UserExercise>().HasKey(sc => new { sc.UserId, sc.ExerciseId });
        modelBuilder.Entity<ExercisePrerequisite>().HasKey(sc => new { sc.ExerciseId, sc.PrerequisiteExerciseId });

        ////////// Query Filters //////////
        modelBuilder.Entity<Exercise>().HasQueryFilter(p => p.DisabledReason == null);
        modelBuilder.Entity<Variation>().HasQueryFilter(p => p.DisabledReason == null);
        modelBuilder.Entity<UserExercise>().HasQueryFilter(p => p.Exercise.DisabledReason == null);
        modelBuilder.Entity<UserVariation>().HasQueryFilter(p => p.Variation.DisabledReason == null);
        modelBuilder.Entity<UserVariationLog>().HasQueryFilter(p => p.UserVariation.Variation.DisabledReason == null);
        modelBuilder.Entity<Instruction>().HasQueryFilter(p => p.DisabledReason == null && p.Variation.DisabledReason == null);
        modelBuilder.Entity<ExercisePrerequisite>().HasQueryFilter(p => p.PrerequisiteExercise.DisabledReason == null && p.Exercise.DisabledReason == null);
        modelBuilder.Entity<UserWorkoutVariation>().HasQueryFilter(p => p.Variation.DisabledReason == null);
        modelBuilder.Entity<UserToken>().HasQueryFilter(p => p.Expires > DateTime.UtcNow);

        ////////// Partial Indexes ////////// Clone existing indexes to have a DisabledReason filter. Only filter out DisabledReason if there's a global query filter set for it.
        modelBuilder.Entity<Exercise>().Metadata.GetIndexes().Where(index => index.GetFilter() == null).ToList().ForEach(index => modelBuilder.Entity<Exercise>().Metadata.AddIndex(index.Properties, $"{index.GetDatabaseName()}_DisabledReason").SetFilter(DISABLED_REASON_IS_NULL));
        modelBuilder.Entity<Variation>().Metadata.GetIndexes().Where(index => index.GetFilter() == null).ToList().ForEach(index => modelBuilder.Entity<Variation>().Metadata.AddIndex(index.Properties, $"{index.GetDatabaseName()}_DisabledReason").SetFilter(DISABLED_REASON_IS_NULL));
        modelBuilder.Entity<Instruction>().Metadata.GetIndexes().Where(index => index.GetFilter() == null).ToList().ForEach(index => modelBuilder.Entity<Instruction>().Metadata.AddIndex(index.Properties, $"{index.GetDatabaseName()}_DisabledReason").SetFilter(DISABLED_REASON_IS_NULL));

        ////////// Conversions //////////
        modelBuilder.Entity<UserWorkout>().OwnsOne(e => e.Rotation).Property(e => e.MuscleGroups).HasConversion(v => JsonSerializer.Serialize(v, JsonSerializerOptions),
            v => JsonSerializer.Deserialize<List<MusculoskeletalSystem>>(v, JsonSerializerOptions)!, new ValueComparer<IList<MusculoskeletalSystem>>((mg, mg2) => mg == mg2, mg => mg.GetHashCode()));
        modelBuilder.Entity<UserFrequency>().OwnsOne(e => e.Rotation).Property(e => e.MuscleGroups).HasConversion(v => JsonSerializer.Serialize(v, JsonSerializerOptions),
            v => JsonSerializer.Deserialize<List<MusculoskeletalSystem>>(v, JsonSerializerOptions)!, new ValueComparer<IList<MusculoskeletalSystem>>((mg, mg2) => mg == mg2, mg => mg.GetHashCode()));
        /*modelBuilder.Entity<Exercise>().Property(e => e.Skills).HasConversion(v => JsonSerializer.Serialize(v, JsonSerializerOptions),
            v => JsonSerializer.Deserialize<Dictionary<string, int>>(v, JsonSerializerOptions)!.Where(kv => Enum.IsDefined(typeof(SkillTypes), kv.Key)).ToDictionary(kv => Enum.Parse<SkillTypes>(kv.Key), kv => kv.Value),
            new ValueComparer<IDictionary<SkillTypes, int>>((mg, mg2) => mg == mg2, mg => mg.GetHashCode()));*/

        // Set the default value of the db columns be attributes.
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            foreach (var property in entityType.GetProperties())
            {
                if (((MemberInfo?)property.PropertyInfo ?? property.FieldInfo) is MemberInfo memberInfo)
                {
                    if (Attribute.GetCustomAttribute(memberInfo, typeof(DefaultValueAttribute)) is DefaultValueAttribute defaultValue)
                    {
                        property.SetDefaultValue(defaultValue.Value);
                    }
                }
            }
        }
    }
}
