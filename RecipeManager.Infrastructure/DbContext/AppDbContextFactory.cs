using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace RecipeManager.Infrastructure.Data
{
    public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
    {
        public AppDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();

            // SQLite is used by both frontend projects, we should use it for migrations to match the target database
            optionsBuilder.UseSqlite("Data Source=recipes.db");

            return new AppDbContext(optionsBuilder.Options);
        }
    }
}
