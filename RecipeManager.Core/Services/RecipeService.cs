using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using RecipeManager.Core.Interfaces;
using RecipeManager.Core.Models;

namespace RecipeManager.Core.Services
{
    public class RecipeService
    {
        private readonly IRecipeRepository _recipeRepository;
        private readonly ILogger<RecipeService> _logger;

        public RecipeService(IRecipeRepository recipeRepository, ILogger<RecipeService> logger)
        {
            _recipeRepository = recipeRepository ?? throw new ArgumentNullException(nameof(recipeRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<List<Recipe>> FindAsync(System.Linq.Expressions.Expression<System.Func<Recipe, bool>> predicate)
        {
            _logger.LogInformation("Filtering recipes.");
            return await _recipeRepository.FindAsync(predicate);
        }

        public async Task<List<Recipe>> GetAllRecipesAsync()
        {
            _logger.LogInformation("Loading all recipes.");
            try
            {
                var recipes = await _recipeRepository.GetAllAsync();
                _logger.LogInformation("Successfully loaded {Count} recipes.", recipes?.Count ?? 0);
                return recipes ?? new List<Recipe>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while loading all recipes.");
                throw;
            }
        }

        public async Task<Recipe?> GetRecipeByIdAsync(int id)
        {
            if (id <= 0)
            {
                _logger.LogWarning("Attempted to load recipe with invalid Id: {Id}", id);
                throw new ArgumentException("Recipe Id must be strictly positive.", nameof(id));
            }

            _logger.LogInformation("Loading recipe with Id: {Id}", id);
            try
            {
                var recipe = await _recipeRepository.GetByIdAsync(id);
                if (recipe == null)
                {
                    _logger.LogWarning("Recipe with Id: {Id} not found.", id);
                }
                else
                {
                    _logger.LogInformation("Successfully loaded recipe with Id: {Id}", id);
                }
                return recipe;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while loading recipe with Id: {Id}", id);
                throw;
            }
        }

        public async Task CreateRecipeAsync(Recipe recipe)
        {
            if (recipe == null)
            {
                var ex = new ArgumentNullException(nameof(recipe));
                _logger.LogError(ex, "Attempted to create a null recipe.");
                throw ex;
            }

            if (string.IsNullOrWhiteSpace(recipe.Title))
            {
                var ex = new ArgumentException("Recipe title cannot be null or empty.", nameof(recipe));
                _logger.LogError(ex, "Attempted to create a recipe with missing or empty title.");
                throw ex;
            }

            _logger.LogInformation("Creating new recipe with title: {Title}", recipe.Title);
            try
            {
                await _recipeRepository.AddAsync(recipe);
                _logger.LogInformation("Successfully created recipe with title: {Title}", recipe.Title);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while creating recipe with title: {Title}", recipe.Title);
                throw;
            }
        }

        public async Task UpdateRecipeAsync(Recipe recipe)
        {
            if (recipe == null)
            {
                var ex = new ArgumentNullException(nameof(recipe));
                _logger.LogError(ex, "Attempted to update a null recipe.");
                throw ex;
            }

            if (string.IsNullOrWhiteSpace(recipe.Title))
            {
                var ex = new ArgumentException("Recipe title cannot be null or empty.", nameof(recipe));
                _logger.LogError(ex, "Attempted to update a recipe with missing or empty title.");
                throw ex;
            }

            if (recipe.Id <= 0)
            {
                var ex = new ArgumentException("Recipe Id must be strictly positive.", nameof(recipe));
                _logger.LogError(ex, "Attempted to update a recipe with invalid Id: {Id}", recipe.Id);
                throw ex;
            }

            _logger.LogInformation("Updating recipe with Id: {Id}", recipe.Id);
            try
            {
                await _recipeRepository.UpdateAsync(recipe);
                _logger.LogInformation("Successfully updated recipe with Id: {Id}", recipe.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while updating recipe with Id: {Id}", recipe.Id);
                throw;
            }
        }

        public async Task DeleteRecipeAsync(int id)
        {
            if (id <= 0)
            {
                var ex = new ArgumentException("Recipe Id must be strictly positive.", nameof(id));
                _logger.LogError(ex, "Attempted to delete recipe with invalid Id: {Id}", id);
                throw ex;
            }

            _logger.LogInformation("Deleting recipe with Id: {Id}", id);
            try
            {
                await _recipeRepository.DeleteAsync(id);
                _logger.LogInformation("Successfully deleted recipe with Id: {Id}", id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while deleting recipe with Id: {Id}", id);
                throw;
            }
        }
    }
}
