using Core.Models.Exercise;
using Data.Entities.Equipment;
using Data.Entities.Exercise;
using Data.Entities.Footnote;
using Data.Entities.Newsletter;
using Data.Entities.User;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Text.Json;

namespace Data;

public class CoreContext : DbContext
{
    public DbSet<User> Users { get; set; } = null!;
    public DbSet<UserToken> UserTokens { get; set; } = null!;
    public DbSet<UserEmail> UserEmails { get; set; } = null!;
    public DbSet<UserFrequency> UserFrequencies { get; set; } = null!;
    public DbSet<UserVariationWeight> UserVariationWeights { get; set; } = null!;
    public DbSet<UserExercise> UserExercises { get; set; } = null!;
    public DbSet<UserVariation> UserVariations { get; set; } = null!;
    public DbSet<UserExerciseVariation> UserExerciseVariations { get; set; } = null!;
    public DbSet<UserMuscleStrength> UserMuscleStrengths { get; set; } = null!;
    public DbSet<UserMuscleMobility> UserMuscleMobilities { get; set; } = null!;
    public DbSet<UserMuscleFlexibility> UserMuscleFlexibilities { get; set; } = null!;
    public DbSet<Variation> Variations { get; set; } = null!;
    public DbSet<Exercise> Exercises { get; set; } = null!;
    public DbSet<ExerciseVariation> ExerciseVariations { get; set; } = null!;
    public DbSet<UserWorkout> UserWorkouts { get; set; } = null!;
    public DbSet<UserWorkoutExerciseVariation> UserWorkoutExerciseVariations { get; set; } = null!;
    public DbSet<Footnote> Footnotes { get; set; } = null!;

    public CoreContext() : base() { }

    public CoreContext(DbContextOptions<CoreContext> context) : base(context) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ////////// Keys //////////
        modelBuilder.Entity<UserFrequency>().HasKey(sc => new { sc.UserId, sc.Id });
        modelBuilder.Entity<UserMuscleStrength>().HasKey(sc => new { sc.UserId, sc.MuscleGroup });
        modelBuilder.Entity<UserMuscleMobility>().HasKey(sc => new { sc.UserId, sc.MuscleGroup });
        modelBuilder.Entity<UserMuscleFlexibility>().HasKey(sc => new { sc.UserId, sc.MuscleGroup });
        modelBuilder.Entity<UserExercise>().HasKey(sc => new { sc.UserId, sc.ExerciseId });
        modelBuilder.Entity<UserVariation>().HasKey(sc => new { sc.UserId, sc.VariationId });
        modelBuilder.Entity<UserExerciseVariation>().HasKey(sc => new { sc.UserId, sc.ExerciseVariationId });
        modelBuilder.Entity<ExercisePrerequisite>().HasKey(sc => new { sc.ExerciseId, sc.PrerequisiteExerciseId });
        //modelBuilder.Entity<ExerciseVariation>().HasKey(sc => new { sc.ExerciseId, sc.VariationId });


        ////////// Conversions //////////
        modelBuilder
            .Entity<UserWorkout>()
            .OwnsOne(e => e.Rotation)
            .Property(e => e.MuscleGroups)
            .HasConversion(v => JsonSerializer.Serialize(v, new JsonSerializerOptions()),
                v => JsonSerializer.Deserialize<List<MuscleGroups>>(v, new JsonSerializerOptions())!,
                new ValueComparer<IList<MuscleGroups>>((mg, mg2) => mg == mg2, mg => mg.GetHashCode())
            );
        modelBuilder
            .Entity<UserFrequency>()
            .OwnsOne(e => e.Rotation)
            .Property(e => e.MuscleGroups)
            .HasConversion(v => JsonSerializer.Serialize(v, new JsonSerializerOptions()),
                v => JsonSerializer.Deserialize<List<MuscleGroups>>(v, new JsonSerializerOptions())!,
                new ValueComparer<IList<MuscleGroups>>((mg, mg2) => mg == mg2, mg => mg.GetHashCode())
            );
        //modelBuilder
        //    .Entity<Variation>()
        //    .Property(e => e.StrengthMuscles)
        //    .HasConversion(v => JsonSerializer.Serialize(v, new JsonSerializerOptions()),
        //        v => JsonSerializer.Deserialize<List<MuscleGroups>>(v, new JsonSerializerOptions()),
        //        new ValueComparer<List<MuscleGroups>>((mg, mg2) => mg == mg2, mg => mg.GetHashCode())
        //    );


        ////////// Query Filters //////////
        modelBuilder.Entity<Exercise>().HasQueryFilter(p => p.DisabledReason == null);
        modelBuilder.Entity<Variation>().HasQueryFilter(p => p.DisabledReason == null);
        modelBuilder.Entity<UserExercise>().HasQueryFilter(p => p.Exercise.DisabledReason == null);
        modelBuilder.Entity<UserVariation>().HasQueryFilter(p => p.Variation.DisabledReason == null);
        modelBuilder.Entity<UserVariationWeight>().HasQueryFilter(p => p.Variation.DisabledReason == null);
        modelBuilder.Entity<UserToken>().HasQueryFilter(p => p.Expires > DateTime.UtcNow);
        modelBuilder.Entity<Instruction>().HasQueryFilter(p => p.DisabledReason == null && p.Variation.DisabledReason == null);
        modelBuilder.Entity<ExercisePrerequisite>().HasQueryFilter(p => p.PrerequisiteExercise.DisabledReason == null && p.Exercise.DisabledReason == null);
        modelBuilder.Entity<ExerciseVariation>().HasQueryFilter(p =>
            p.DisabledReason == null
            && p.Exercise.DisabledReason == null
            && p.Variation.DisabledReason == null
        );
        modelBuilder.Entity<UserExerciseVariation>().HasQueryFilter(p =>
            p.ExerciseVariation.DisabledReason == null
            && p.ExerciseVariation.Exercise.DisabledReason == null
            && p.ExerciseVariation.Variation.DisabledReason == null
        );
        modelBuilder.Entity<UserWorkoutExerciseVariation>().HasQueryFilter(p =>
            p.ExerciseVariation.DisabledReason == null
            && p.ExerciseVariation.Exercise.DisabledReason == null
            && p.ExerciseVariation.Variation.DisabledReason == null
        );
    }
}
