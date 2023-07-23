using Data.Entities.Equipment;
using Data.Entities.Exercise;
using Data.Entities.Footnote;
using Data.Entities.Newsletter;
using Data.Entities.User;
using Microsoft.EntityFrameworkCore;

namespace Data.Data;

public class CoreContext : DbContext
{
    public DbSet<User> Users { get; set; } = null!;
    public DbSet<UserToken> UserTokens { get; set; } = null!;
    public DbSet<Equipment> Equipment { get; set; } = null!;
    public DbSet<UserEquipment> UserEquipments { get; set; } = null!;
    public DbSet<UserEmail> UserEmails { get; set; } = null!;
    public DbSet<UserFrequency> UserFrequencies { get; set; } = null!;
    public DbSet<UserVariationWeight> UserVariationWeights { get; set; } = null!;
    public DbSet<UserExercise> UserExercises { get; set; } = null!;
    public DbSet<UserVariation> UserVariations { get; set; } = null!;
    public DbSet<UserExerciseVariation> UserExerciseVariations { get; set; } = null!;
    public DbSet<UserMuscleStrength> UserMuscleStrengths { get; set; } = null!;
    public DbSet<UserMuscleMobility> UserMuscleMobilities { get; set; } = null!;
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
        modelBuilder.Entity<UserEquipment>().HasKey(sc => new { sc.UserId, sc.EquipmentId });
        modelBuilder.Entity<UserFrequency>().HasKey(sc => new { sc.UserId, sc.Id });
        modelBuilder.Entity<UserMuscleStrength>().HasKey(sc => new { sc.UserId, sc.MuscleGroup });
        modelBuilder.Entity<UserMuscleMobility>().HasKey(sc => new { sc.UserId, sc.MuscleGroup });
        modelBuilder.Entity<UserExercise>().HasKey(sc => new { sc.UserId, sc.ExerciseId });
        modelBuilder.Entity<UserVariation>().HasKey(sc => new { sc.UserId, sc.VariationId });
        modelBuilder.Entity<UserExerciseVariation>().HasKey(sc => new { sc.UserId, sc.ExerciseVariationId });
        modelBuilder.Entity<InstructionLocation>().HasKey(sc => new { sc.Location, sc.InstructionId });
        modelBuilder.Entity<ExercisePrerequisite>().HasKey(sc => new { sc.ExerciseId, sc.PrerequisiteExerciseId });
        //modelBuilder.Entity<ExerciseVariation>().HasKey(sc => new { sc.ExerciseId, sc.VariationId });

        ////////// Query Filters //////////
        modelBuilder.Entity<Exercise>().HasQueryFilter(p => p.DisabledReason == null);
        modelBuilder.Entity<Variation>().HasQueryFilter(p => p.DisabledReason == null);
        modelBuilder.Entity<Equipment>().HasQueryFilter(p => p.DisabledReason == null);
        modelBuilder.Entity<UserExercise>().HasQueryFilter(p => p.Exercise.DisabledReason == null);
        modelBuilder.Entity<UserEquipment>().HasQueryFilter(p => p.Equipment.DisabledReason == null);
        modelBuilder.Entity<UserVariation>().HasQueryFilter(p => p.Variation.DisabledReason == null);
        modelBuilder.Entity<UserVariationWeight>().HasQueryFilter(p => p.Variation.DisabledReason == null);
        modelBuilder.Entity<InstructionLocation>().HasQueryFilter(p => p.Instruction.DisabledReason == null);
        modelBuilder.Entity<UserToken>().HasQueryFilter(p => p.Expires > DateTime.UtcNow);
        modelBuilder.Entity<Intensity>().HasQueryFilter(p => p.DisabledReason == null && p.Variation.DisabledReason == null);
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

        ////////// Auto Includes //////////
        // Instructions are never complete without their Locations if there are any
        modelBuilder.Entity<Instruction>().Navigation(d => d.Locations).AutoInclude();
        // Instructions are never complete without their Equipment if there are any
        modelBuilder.Entity<Instruction>().Navigation(d => d.Equipment).AutoInclude();

        ////////// Table Mappings //////////
        modelBuilder.Entity<Instruction>()
            .HasMany(p => p.Equipment)
            .WithMany(p => p.Instructions)
            .UsingEntity(j => j.ToTable("instruction_equipment"));
    }
}
