using FinerFettle.Functions.Models.User;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using System.Reflection.Metadata;

namespace FinerFettle.Functions.Data
{
    public class CoreContext : DbContext
    {
        public DbSet<User> Users { get; set; } = null!;

        public CoreContext() : base() { }

        public CoreContext(DbContextOptions<CoreContext> context) : base(context) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
        }
    }
}
