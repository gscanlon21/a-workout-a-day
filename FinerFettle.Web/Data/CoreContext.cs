using FinerFettle.Web.Models.Newsletter;
using FinerFettle.Web.Models.User;
using FinerFettle.Web.Models.Exercise;
using Microsoft.EntityFrameworkCore;
using FinerFettle.Web.Models.Footnotes;
using Microsoft.Extensions.Hosting;

namespace FinerFettle.Web.Data
{
    public class CoreContext : DbContext
    {
        public DbSet<User> Users { get; set; } = null!;
        public DbSet<Equipment> Equipment { get; set; } = null!;
        public DbSet<ExerciseUserProgression> UserProgressions { get; set; } = null!;
        public DbSet<UserVariation> UserVariations { get; set; } = null!;
        public DbSet<UserIntensity> UserIntensities { get; set; } = null!;
        public DbSet<Intensity> Intensities { get; set; } = null!;
        public DbSet<Exercise> Exercises { get; set; } = null!;
        public DbSet<Newsletter> Newsletters { get; set; } = null!;
        public DbSet<Footnote> Footnotes { get; set; } = null!;

        public CoreContext() : base() { }

        public CoreContext(DbContextOptions<CoreContext> context) : base(context) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<EquipmentUser>().HasKey(sc => new { sc.UserId, sc.EquipmentId });
            modelBuilder.Entity<ExerciseUserProgression>().HasKey(sc => new { sc.UserId, sc.ExerciseId });
            modelBuilder.Entity<UserVariation>().HasKey(sc => new { sc.UserId, sc.VariationId });
            modelBuilder.Entity<UserIntensity>().HasKey(sc => new { sc.UserId, sc.IntensityId });

            modelBuilder.Entity<Intensity>().HasQueryFilter(p => p.DisabledReason == null);
            modelBuilder.Entity<Variation>().HasQueryFilter(p => p.DisabledReason == null);
            modelBuilder.Entity<Exercise>().HasQueryFilter(p => p.DisabledReason == null);
            modelBuilder.Entity<UserVariation>().HasQueryFilter(p => p.Variation.DisabledReason == null);
            modelBuilder.Entity<UserIntensity>().HasQueryFilter(p => p.Intensity.DisabledReason == null);
            modelBuilder.Entity<ExerciseUserProgression>().HasQueryFilter(p => p.Exercise.DisabledReason == null);
            modelBuilder.Entity<IntensityPreference>().HasQueryFilter(p => p.Intensity.DisabledReason == null);
            modelBuilder.Entity<EquipmentGroup>().HasQueryFilter(p => p.Intensity.DisabledReason == null);
        }
    }
}
