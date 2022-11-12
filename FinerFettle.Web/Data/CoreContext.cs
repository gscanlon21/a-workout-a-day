using Microsoft.EntityFrameworkCore;
using FinerFettle.Web.Entities.Exercise;
using FinerFettle.Web.Entities.Equipment;
using FinerFettle.Web.Entities.Footnote;
using FinerFettle.Web.Entities.Newsletter;
using FinerFettle.Web.Entities.User;

namespace FinerFettle.Web.Data;

public class CoreContext : DbContext
{
    public DbSet<User> Users { get; set; } = null!;
    public DbSet<Equipment> Equipment { get; set; } = null!;
    public DbSet<UserExercise> UserExercises { get; set; } = null!;
    public DbSet<UserVariation> UserVariations { get; set; } = null!;
    public DbSet<UserExerciseVariation> UserExerciseVariations { get; set; } = null!;
    public DbSet<Variation> Variations { get; set; } = null!;
    public DbSet<Exercise> Exercises { get; set; } = null!;
    public DbSet<ExerciseVariation> ExerciseVariations { get; set; } = null!;
    public DbSet<Newsletter> Newsletters { get; set; } = null!;
    public DbSet<NewsletterVariation> NewsletterVariations { get; set; } = null!;
    public DbSet<Footnote> Footnotes { get; set; } = null!;

    public CoreContext() : base() { }

    public CoreContext(DbContextOptions<CoreContext> context) : base(context) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<UserEquipment>().HasKey(sc => new { sc.UserId, sc.EquipmentId });
        modelBuilder.Entity<UserExercise>().HasKey(sc => new { sc.UserId, sc.ExerciseId });
        modelBuilder.Entity<UserVariation>().HasKey(sc => new { sc.UserId, sc.VariationId });
        modelBuilder.Entity<UserExerciseVariation>().HasKey(sc => new { sc.UserId, sc.ExerciseVariationId });
        modelBuilder.Entity<UserToken>().HasKey(sc => new { sc.UserId, sc.Token });
        modelBuilder.Entity<ExercisePrerequisite>().HasKey(sc => new { sc.ExerciseId, sc.PrerequisiteExerciseId });
        //modelBuilder.Entity<ExerciseVariation>().HasKey(sc => new { sc.ExerciseId, sc.VariationId });

        modelBuilder.Entity<Exercise>().HasQueryFilter(p => p.DisabledReason == null);
        modelBuilder.Entity<ExercisePrerequisite>().HasQueryFilter(p => p.PrerequisiteExercise.DisabledReason == null && p.Exercise.DisabledReason == null);
        modelBuilder.Entity<ExerciseVariation>().HasQueryFilter(p => p.Exercise.DisabledReason == null && p.Variation.DisabledReason == null);
        modelBuilder.Entity<Variation>().HasQueryFilter(p => p.DisabledReason == null);
        modelBuilder.Entity<Intensity>().HasQueryFilter(p => p.Variation.DisabledReason == null && p.DisabledReason == null);
        // Can't use a global query filter on Equipment or else p.Equipment.Count would always be zero if all the EquipmentGroup's Equipment is disabled.
        modelBuilder.Entity<EquipmentGroup>().HasQueryFilter(p => p.DisabledReason == null && p.Variation.DisabledReason == null && (p.Equipment.Count == 0 || p.Equipment.Any(e => e.DisabledReason == null)));
        modelBuilder.Entity<UserEquipment>().HasQueryFilter(p => p.Equipment.DisabledReason == null);
        modelBuilder.Entity<UserExercise>().HasQueryFilter(p => p.Exercise.DisabledReason == null);
        modelBuilder.Entity<UserExerciseVariation>().HasQueryFilter(p => p.ExerciseVariation.Exercise.DisabledReason == null && p.ExerciseVariation.Variation.DisabledReason == null);
        modelBuilder.Entity<UserVariation>().HasQueryFilter(p => p.Variation.DisabledReason == null);           
        modelBuilder.Entity<UserToken>().HasQueryFilter(p => p.Expires > DateOnly.FromDateTime(DateTime.UtcNow));
        modelBuilder.Entity<NewsletterVariation>().HasQueryFilter(p => p.Variation.DisabledReason == null);

        modelBuilder.Entity<EquipmentGroup>()
            .HasMany(p => p.Equipment)
            .WithMany(p => p.EquipmentGroups)
            .UsingEntity(j => j.ToTable("equipment_group_equipment"));
    }
}
