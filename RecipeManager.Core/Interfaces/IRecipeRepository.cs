using System.Threading.Tasks;
using RecipeManager.Core.Models;

namespace RecipeManager.Core.Interfaces
{
    public interface IRecipeRepository : IRepository<Recipe>
    {
        Task<Recipe?> GetWithRevisionsAsync(int id);
    }
}