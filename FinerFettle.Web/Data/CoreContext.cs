using FinerFettle.Web.Models.Newsletter;
using FinerFettle.Web.Models.User;
using FinerFettle.Web.Models.Exercise;
using Microsoft.EntityFrameworkCore;
using FinerFettle.Web.Models.Footnotes;

namespace FinerFettle.Web.Data
{
    public class CoreContext : DbContext
    {
        public DbSet<User> Users { get; set; } = null!;
        public DbSet<Equipment> Equipment { get; set; } = null!;
        public DbSet<Exercise> Exercises { get; set; } = null!;
        public DbSet<ExerciseUserProgression> UserProgressions { get; set; } = null!;
        public DbSet<Variation> Variations { get; set; } = null!;
        public DbSet<Intensity> Intensities { get; set; } = null!;
        public DbSet<Newsletter> Newsletters { get; set; } = null!;
        public DbSet<Footnote> Footnotes { get; set; } = null!;

        public CoreContext() : base() { }

        public CoreContext(DbContextOptions<CoreContext> context) : base(context) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<EquipmentUser>().HasKey(sc => new { sc.EquipmentId, sc.UserId });
            modelBuilder.Entity<ExerciseUserProgression>().HasKey(sc => new { sc.ExerciseId, sc.UserId });
        }
    }
}
