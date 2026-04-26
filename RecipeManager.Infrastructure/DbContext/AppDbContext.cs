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
                new Category { Id = 1, Name = "Breakfast", Description = "Start your day right", CommonIngredients = "Eggs, Bacon, Oats" },
                new Category { Id = 2, Name = "Lunch", Description = "Mid-day meals", CommonIngredients = "Chicken, Rice, Bread" },
                new Category { Id = 3, Name = "Dinner", Description = "Evening dining", CommonIngredients = "Pork, Beef, Potatoes" },
                new Category { Id = 4, Name = "Dessert", Description = "Sweet treats", CommonIngredients = "Sugar, Flour, Chocolate" },
                new Category { Id = 5, Name = "Other", Description = "Miscellaneous recipes", CommonIngredients = "Various" }
            );
        }
    }
}