using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RecipeManager.Core.Interfaces;
using RecipeManager.Core.Models;
using RecipeManager.Infrastructure.Data;

namespace RecipeManager.Infrastructure.Repositories
{
    public class RecipeRepository : Repository<Recipe>, IRecipeRepository
    {
        public RecipeRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<Recipe?> GetWithRevisionsAsync(int id)
        {
            try
            {
                return await _dbSet
                    .Include(r => r.Revisions)
                    .Include(r => r.Category)
                    .FirstOrDefaultAsync(r => r.Id == id);
            }
            catch (System.Exception ex)
            {
                 throw new System.Exception("A database error occurred while fetching the recipe with revisions.", ex);
            }
        }
    }
}