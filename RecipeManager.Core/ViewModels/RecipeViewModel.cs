using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RecipeManager.Core.Models;
using RecipeManager.Core.Services;

namespace RecipeManager.Core.ViewModels
{
    public partial class RecipeViewModel : ObservableObject
    {
        private readonly RecipeService _recipeService;

        [ObservableProperty]
        private ObservableCollection<Recipe> _recipes;

        [ObservableProperty]
        private Recipe? _selectedRecipe;

        public RecipeViewModel(RecipeService recipeService)
        {
            _recipeService = recipeService;
            _recipes = new ObservableCollection<Recipe>();
        }

        [RelayCommand]
        private async Task LoadRecipesAsync()
        {
            var loadedRecipes = await _recipeService.GetAllRecipesAsync();
            Recipes.Clear();
            foreach (var recipe in loadedRecipes)
            {
                Recipes.Add(recipe);
            }
        }

        [RelayCommand]
        private async Task AddRecipeAsync()
        {
            if (SelectedRecipe == null) return;

            SaveState();
            await _recipeService.CreateRecipeAsync(SelectedRecipe);
            if (!Recipes.Contains(SelectedRecipe))
            {
                Recipes.Add(SelectedRecipe);
            }
        }

        [RelayCommand]
        private async Task UpdateRecipeAsync()
        {
            if (SelectedRecipe == null) return;

            SaveState();
            await _recipeService.UpdateRecipeAsync(SelectedRecipe);
        }

        [RelayCommand]
        private async Task DeleteRecipeAsync()
        {
            if (SelectedRecipe == null) return;

            SaveState();
            await _recipeService.DeleteRecipeAsync(SelectedRecipe.Id);
            Recipes.Remove(SelectedRecipe);
            SelectedRecipe = null;
        }

        private readonly Stack<string> _undoStack = new Stack<string>();
        private readonly Stack<string> _redoStack = new Stack<string>();
        private readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions { ReferenceHandler = ReferenceHandler.IgnoreCycles };

        private void SaveState()
        {
            var state = JsonSerializer.Serialize(Recipes, _jsonOptions);
            _undoStack.Push(state);
            _redoStack.Clear();
            UndoCommand.NotifyCanExecuteChanged();
            RedoCommand.NotifyCanExecuteChanged();
        }

        private bool CanUndo() => _undoStack.Count > 0;

        [RelayCommand(CanExecute = nameof(CanUndo))]
        private void Undo()
        {
            if (_undoStack.Count > 0)
            {
                _redoStack.Push(JsonSerializer.Serialize(Recipes, _jsonOptions));
                RestoreState(_undoStack.Pop());
                UndoCommand.NotifyCanExecuteChanged();
                RedoCommand.NotifyCanExecuteChanged();
            }
        }

        private bool CanRedo() => _redoStack.Count > 0;

        [RelayCommand(CanExecute = nameof(CanRedo))]
        private void Redo()
        {
            if (_redoStack.Count > 0)
            {
                _undoStack.Push(JsonSerializer.Serialize(Recipes, _jsonOptions));
                RestoreState(_redoStack.Pop());
                UndoCommand.NotifyCanExecuteChanged();
                RedoCommand.NotifyCanExecuteChanged();
            }
        }

        private void RestoreState(string state)
        {
            var recipesList = JsonSerializer.Deserialize<List<Recipe>>(state, _jsonOptions);
            Recipes.Clear();
            if (recipesList != null)
            {
                foreach (var recipe in recipesList)
                {
                    Recipes.Add(recipe);
                }
            }
        }
    }
}
