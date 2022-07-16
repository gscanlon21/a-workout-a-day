using FinerFettle.Web.Models;
using Microsoft.EntityFrameworkCore;
using System.Configuration;

namespace FinerFettle.Web.Data
{
    public class NewsletterContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Exercise> Exercises { get; set; }

        public NewsletterContext() : base() { }

        public NewsletterContext(DbContextOptions<NewsletterContext> context) : base(context) { }
    }
}
