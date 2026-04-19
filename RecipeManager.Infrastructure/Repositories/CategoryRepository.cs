using RecipeManager.Core.Interfaces;
using RecipeManager.Core.Models;
using RecipeManager.Infrastructure.Data;

namespace RecipeManager.Infrastructure.Repositories
{
    public class CategoryRepository : Repository<Category>, ICategoryRepository
    {
        public CategoryRepository(AppDbContext context) : base(context)
        {
        }
    }
}