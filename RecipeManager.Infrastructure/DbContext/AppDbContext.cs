using Microsoft.EntityFrameworkCore;
using RecipeManager.Core.Models;

namespace RecipeManager.Infrastructure.Data
{
    public class AppDbContext : Microsoft.EntityFrameworkCore.DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Recipe> Recipes { get; set; }
        public DbSet<Revision> Revisions { get; set; }
        public DbSet<Category> Categories { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Recipe>()
                .HasMany(r => r.Revisions)
                .WithOne(rv => rv.Recipe)
                .HasForeignKey(rv => rv.RecipeId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Recipe>()
                .HasOne(r => r.Category)
                .WithMany(c => c.Recipes)
                .HasForeignKey(r => r.CategoryId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Category>().HasData(
                new Category { Id = 1, Name = "Breakfast" },
                new Category { Id = 2, Name = "Lunch" },
                new Category { Id = 3, Name = "Dinner" },
                new Category { Id = 4, Name = "Dessert" },
                new Category { Id = 5, Name = "Other" }
            );
        }
    }
}