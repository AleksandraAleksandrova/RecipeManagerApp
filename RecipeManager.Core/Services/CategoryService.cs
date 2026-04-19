using System.Collections.Generic;
using System.Threading.Tasks;
using RecipeManager.Core.Interfaces;
using RecipeManager.Core.Models;

namespace RecipeManager.Core.Services
{
    public class CategoryService
    {
        private readonly ICategoryRepository _categoryRepository;

        public CategoryService(ICategoryRepository categoryRepository)
        {
            _categoryRepository = categoryRepository;
        }

        public async Task<IEnumerable<Category>> FindAsync(System.Linq.Expressions.Expression<System.Func<Category, bool>> predicate)
        {
            return await _categoryRepository.FindAsync(predicate);
        }

        public async Task<IEnumerable<Category>> GetAllCategoriesAsync()
        {
            return await _categoryRepository.GetAllAsync();
        }

        public Task CreateCategoryAsync(Category category)
        {
            return _categoryRepository.AddAsync(category);
        }
    }
}