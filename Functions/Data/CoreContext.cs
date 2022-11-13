using FinerFettle.Functions.Models.Newsletter;
using FinerFettle.Functions.Models.User;
using Microsoft.EntityFrameworkCore;

namespace FinerFettle.Functions.Data;

public class CoreContext : DbContext
{
    public DbSet<User> Users { get; set; } = null!;
    public DbSet<Newsletter> Newsletters { get; set; } = null!;
    public DbSet<UserToken> UserTokens { get; set; } = null!;

    public CoreContext() : base() { }

    public CoreContext(DbContextOptions<CoreContext> context) : base(context) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<UserToken>().HasKey(sc => new { sc.UserId, sc.Token });
    }
}
