using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using RecipeManager.Core.Models;
using RecipeManager.Core.Services;
using RecipeManager.Core.Interfaces;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace RecipeManager.Core.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IDialogService _dialogService;

        [ObservableProperty]
        private ObservableCollection<Recipe> _recipes;

        [ObservableProperty]
        private ObservableCollection<Category> _categories;

        [ObservableProperty]
        private List<string> _recipeListColumns = new() { "Title" };

        [ObservableProperty]
        private List<string> _categoryListColumns = new() { "Name" };

        [ObservableProperty]
        private Recipe _selectedRecipe;

        [ObservableProperty]
        private bool _isCreatingMode;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(CreateRecipeCommand))]
        private string _newRecipeName = string.Empty;

        [ObservableProperty]
        private string _newRecipeDescription = string.Empty;

        [ObservableProperty]
        private string _newRecipeIngredients = string.Empty;

        [ObservableProperty]
        private string _newRecipeInstructions = string.Empty;

        [ObservableProperty]
        private int? _newRecipeCategoryId;

        [ObservableProperty]
        private int? _selectedFilterCategoryId;

        [ObservableProperty]
        private string _searchRecipeTitle = string.Empty;

        [ObservableProperty]
        private string _sortRecipeOrder = string.Empty;

        private bool _suppressFilterTrigger = false;

        async partial void OnSortRecipeOrderChanged(string value)
        {
            if (!_suppressFilterTrigger)
                await ApplyRecipeFiltersAsync();
        }

        async partial void OnSearchRecipeTitleChanged(string value)
        {
            if (!_suppressFilterTrigger)
                await ApplyRecipeFiltersAsync();
        }

        async partial void OnSelectedFilterCategoryIdChanged(int? value)
        {
            if (!_suppressFilterTrigger)
                await ApplyRecipeFiltersAsync();
        }

        public async Task ApplyRecipeFiltersAsync()
        {
            if (string.IsNullOrWhiteSpace(SearchRecipeTitle) && !SelectedFilterCategoryId.HasValue)
            {
                await LoadRecipesAsync();
                return;
            }

            var lowerTitle = SearchRecipeTitle?.ToLower() ?? string.Empty;

            await FilterRecipesAsync(r => 
                (string.IsNullOrWhiteSpace(lowerTitle) || r.Title.ToLower().Contains(lowerTitle)) &&
                (!SelectedFilterCategoryId.HasValue || r.CategoryId == SelectedFilterCategoryId.Value));
        }

        [RelayCommand]
        public async Task ClearRecipeFilter()
        {
            _suppressFilterTrigger = true;
            SearchRecipeTitle = string.Empty;
            SelectedFilterCategoryId = null;
            SortRecipeOrder = string.Empty;
            _suppressFilterTrigger = false;
            await ApplyRecipeFiltersAsync();
        }

        [ObservableProperty]
        private string _errorMessage = string.Empty;

        [ObservableProperty]
        private string _successMessage = string.Empty;

        private Recipe _originalRecipe;
        private int _messageToken;

        private async void ShowEphemeralMessage(string successMessage, string errorMessage, int delayMs = 3000)
        {
            ErrorMessage = errorMessage;
            SuccessMessage = successMessage;

            if (string.IsNullOrEmpty(successMessage) && string.IsNullOrEmpty(errorMessage))
                return;

            int currentToken = ++_messageToken;
            await Task.Delay(delayMs);

            if (_messageToken == currentToken)
            {
                ErrorMessage = string.Empty;
                SuccessMessage = string.Empty;
            }
        }

        public MainViewModel(IServiceScopeFactory scopeFactory, IDialogService dialogService)
        {
            _scopeFactory = scopeFactory ?? throw new ArgumentNullException(nameof(scopeFactory));
            _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
            _recipes = new ObservableCollection<Recipe>();
            _categories = new ObservableCollection<Category>();

            _ = InitializeAsync();
        }

        private async Task InitializeAsync()
        {
            await LoadRecipesAsync();
            await LoadCategoriesAsync();
        }

        [RelayCommand]
        public async Task LoadCategoriesAsync()
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var categoryService = scope.ServiceProvider.GetRequiredService<CategoryService>();

                var loadedCategories = await categoryService.GetAllCategoriesAsync();
                Categories.Clear();
                foreach (var category in loadedCategories)
                {
                    Categories.Add(category);
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Failed to load categories: {ex.Message}";
            }
        }

        [RelayCommand]
        public async Task FilterCategoriesAsync(Expression<Func<Category, bool>> predicate)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var categoryService = scope.ServiceProvider.GetRequiredService<CategoryService>();

                var filteredCategories = await categoryService.FindAsync(predicate);
                Categories.Clear();
                foreach (var category in filteredCategories)
                {
                    Categories.Add(category);
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Failed to filter categories: {ex.Message}";
            }
        }

        [ObservableProperty]
        private string _searchCategoryName = string.Empty;

        private bool _suppressCategoryFilter = false;

        async partial void OnSearchCategoryNameChanged(string value)
        {
            if (_suppressCategoryFilter) return;

            if (string.IsNullOrWhiteSpace(value))
            {
                await LoadCategoriesAsync();
            }
            else
            {
                var lowerValue = value.ToLower();
                await FilterCategoriesAsync(c => c.Name.ToLower().Contains(lowerValue));
            }
        }

        [RelayCommand]
        public async Task ClearCategoryFilter()
        {
            _suppressCategoryFilter = true;
            SearchCategoryName = string.Empty;
            _suppressCategoryFilter = false;
            await LoadCategoriesAsync();
        }

        private void ApplySortingToRecipesCollection(IEnumerable<Recipe> source)
        {
            IEnumerable<Recipe> sorted = source;
            if (SortRecipeOrder == "NameAsc") sorted = sorted.OrderBy(r => r.Title);
            else if (SortRecipeOrder == "NameDesc") sorted = sorted.OrderByDescending(r => r.Title);
            else if (SortRecipeOrder == "CreatedAsc") sorted = sorted.OrderBy(r => r.CreatedAt);
            else if (SortRecipeOrder == "CreatedDesc") sorted = sorted.OrderByDescending(r => r.CreatedAt);

            Recipes.Clear();
            foreach (var recipe in sorted)
            {
                Recipes.Add(recipe);
            }
        }

        [RelayCommand]
        public async Task LoadRecipesAsync()
        {
            try
            {
                ErrorMessage = string.Empty;
                using var scope = _scopeFactory.CreateScope();
                var recipeService = scope.ServiceProvider.GetRequiredService<RecipeService>();

                var loadedRecipes = await recipeService.GetAllRecipesAsync();
                ApplySortingToRecipesCollection(loadedRecipes);
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Failed to load recipes: {ex.Message}";
            }
        }

        [RelayCommand]
        public async Task FilterRecipesAsync(Expression<Func<Recipe, bool>> predicate)
        {
            try
            {
                ErrorMessage = string.Empty;
                using var scope = _scopeFactory.CreateScope();
                var recipeService = scope.ServiceProvider.GetRequiredService<RecipeService>();

                var filteredRecipes = await recipeService.FindAsync(predicate);
                ApplySortingToRecipesCollection(filteredRecipes);
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Failed to filter recipes: {ex.Message}";
            }
        }

        async partial void OnSelectedRecipeChanged(Recipe value)
        {
            if (value != null)
            {
                IsCreatingMode = false;
                try
                {
                    using var scope = _scopeFactory.CreateScope();
                    var recipeRepo = scope.ServiceProvider.GetRequiredService<IRecipeRepository>();
                    var fullRecipe = await recipeRepo.GetWithRevisionsAsync(value.Id);

                    if (fullRecipe != null && fullRecipe.Revisions != null)
                    {
                        value.Revisions = new ObservableCollection<Revision>(System.Linq.Enumerable.Reverse(fullRecipe.Revisions));
                        
                        _originalRecipe = new Recipe
                        {
                            Id = fullRecipe.Id,
                            Title = fullRecipe.Title,
                            Description = fullRecipe.Description,
                            Ingredients = fullRecipe.Ingredients,
                            Instructions = fullRecipe.Instructions,
                            CategoryId = fullRecipe.CategoryId
                        };

                        OnPropertyChanged(nameof(SelectedRecipe));
                    }
                }
                catch (Exception ex)
                {
                    ErrorMessage = $"Failed to load revisions: {ex.Message}";
                }
            }
        }

        [RelayCommand]
        public void BeginCreateRecipe()
        {
            SelectedRecipe = null;
            NewRecipeName = string.Empty;
            NewRecipeDescription = string.Empty;
            NewRecipeIngredients = string.Empty;
            NewRecipeInstructions = string.Empty;
            NewRecipeCategoryId = null;
            IsCreatingMode = true;
            ErrorMessage = string.Empty;
            SuccessMessage = string.Empty;
        }

        [RelayCommand]
        public void CancelCreate()
        {
            IsCreatingMode = false;
            ErrorMessage = string.Empty;
            SuccessMessage = string.Empty;
        }

        private bool CanCreateRecipe() => !string.IsNullOrWhiteSpace(NewRecipeName);

        [RelayCommand(CanExecute = nameof(CanCreateRecipe))]
        public async Task CreateRecipeAsync()
        {
            try
            {
                ErrorMessage = string.Empty;
                SuccessMessage = string.Empty;
                var newRecipe = new Recipe
                {
                    Title = NewRecipeName,
                    Description = NewRecipeDescription,
                    Ingredients = NewRecipeIngredients,
                    Instructions = NewRecipeInstructions,
                    CategoryId = NewRecipeCategoryId,
                    CreatedAt = DateTime.UtcNow
                };

                using var scope = _scopeFactory.CreateScope();
                var recipeService = scope.ServiceProvider.GetRequiredService<RecipeService>();
                await recipeService.CreateRecipeAsync(newRecipe);

                NewRecipeName = string.Empty;
                NewRecipeDescription = string.Empty;
                NewRecipeIngredients = string.Empty;
                NewRecipeInstructions = string.Empty;
                NewRecipeCategoryId = null;
                IsCreatingMode = false;
                
                await LoadRecipesAsync();
                
                // Add a small delay so OnPropertyChanged has time to propagate in Blazor before navigation
                await Task.Delay(50);
                
                ShowEphemeralMessage("Recipe successfully created!", string.Empty);
            }
            catch (Exception ex)
            {
                ShowEphemeralMessage(string.Empty, $"Failed to create recipe: {ex.Message}");
            }
        }

        [RelayCommand]
        public async Task UpdateRecipeAsync()
        {
            if (SelectedRecipe == null) return;

            try
            {
                ErrorMessage = string.Empty;
                SuccessMessage = string.Empty;

                var diffs = new List<DiffItem>();
                if (_originalRecipe != null)
                {
                    if (_originalRecipe.Title != SelectedRecipe.Title) 
                        diffs.Add(new DiffItem { FieldName = "Title", OldValue = _originalRecipe.Title, NewValue = SelectedRecipe.Title });
                    if (_originalRecipe.Description != SelectedRecipe.Description) 
                        diffs.Add(new DiffItem { FieldName = "Description", OldValue = _originalRecipe.Description ?? "", NewValue = SelectedRecipe.Description ?? "" });
                    if (_originalRecipe.Ingredients != SelectedRecipe.Ingredients) 
                        diffs.Add(new DiffItem { FieldName = "Ingredients", OldValue = _originalRecipe.Ingredients ?? "", NewValue = SelectedRecipe.Ingredients ?? "" });
                    if (_originalRecipe.Instructions != SelectedRecipe.Instructions) 
                        diffs.Add(new DiffItem { FieldName = "Instructions", OldValue = _originalRecipe.Instructions ?? "", NewValue = SelectedRecipe.Instructions ?? "" });
                    if (_originalRecipe.CategoryId != SelectedRecipe.CategoryId) 
                    {
                        var oldCatName = Categories.FirstOrDefault(c => c.Id == _originalRecipe.CategoryId)?.Name ?? "None";
                        var newCatName = Categories.FirstOrDefault(c => c.Id == SelectedRecipe.CategoryId)?.Name ?? "None";
                        diffs.Add(new DiffItem { FieldName = "Category", OldValue = oldCatName, NewValue = newCatName });
                    }
                }

                if (diffs.Count == 0)
                {
                    ErrorMessage = "No changes detected.";
                    return;
                }

                string content = System.Text.Json.JsonSerializer.Serialize(diffs);
                int revNumber = (SelectedRecipe.Revisions?.Count ?? 0) + 1;

                var revision = new Revision
                {
                    Content = content,
                    ModifiedAt = DateTime.UtcNow,
                    ModifiedBy = $"Revision #{revNumber}",
                    RecipeId = SelectedRecipe.Id
                };

                if (SelectedRecipe.Revisions == null)
                    SelectedRecipe.Revisions = new ObservableCollection<Revision>();

                SelectedRecipe.Revisions.Add(revision);

                using var scope = _scopeFactory.CreateScope();
                var recipeService = scope.ServiceProvider.GetRequiredService<RecipeService>();
                await recipeService.UpdateRecipeAsync(SelectedRecipe);

                // Re-fetch to ensure revisions update in UI
                var recipeRepo = scope.ServiceProvider.GetRequiredService<IRecipeRepository>();
                var fullRecipe = await recipeRepo.GetWithRevisionsAsync(SelectedRecipe.Id);
                if (fullRecipe != null)
                {
                    if (fullRecipe.Revisions != null)
                        fullRecipe.Revisions = new ObservableCollection<Revision>(System.Linq.Enumerable.Reverse(fullRecipe.Revisions));
                        
                    var index = Recipes.IndexOf(SelectedRecipe);
                    if (index >= 0)
                    {
                        Recipes[index] = fullRecipe;
                        SelectedRecipe = fullRecipe;
                    }
                }

                ShowEphemeralMessage("Recipe successfully updated!", string.Empty);
            }
            catch (Exception ex)
            {
                ShowEphemeralMessage(string.Empty, $"Failed to update recipe: {ex.Message}");
            }
        }

        [RelayCommand]
        public async Task DeleteRecipeAsync()
        {
            if (SelectedRecipe == null) return;

            bool isConfirmed = await _dialogService.ShowConfirmDeleteAsync($"You are about to delete the recipe '{SelectedRecipe.Title}'. This action cannot be undone.");
            if (!isConfirmed) return;

            try
            {
                using var scope = _scopeFactory.CreateScope();
                var recipeService = scope.ServiceProvider.GetRequiredService<RecipeService>();
                await recipeService.DeleteRecipeAsync(SelectedRecipe.Id);

                SelectedRecipe = null;
                await LoadRecipesAsync();

                ShowEphemeralMessage("Recipe successfully deleted!", string.Empty);
            }
            catch (Exception ex)
            {
                ShowEphemeralMessage(string.Empty, $"Failed to delete recipe: {ex.Message}");
            }
        }

        [RelayCommand]
        public async Task ViewRevision(Revision revision)
        {
            if (revision != null)
            {
                try
                {
                    var diffs = System.Text.Json.JsonSerializer.Deserialize<List<DiffItem>>(revision.Content);
                    if (diffs != null && diffs.Count > 0)
                    {
                        foreach (var diff in diffs)
                        {
                            if (diff.FieldName == "CategoryId")
                            {
                                diff.FieldName = "Category";
                                if (int.TryParse(diff.OldValue, out int oldId))
                                {
                                    diff.OldValue = Categories.FirstOrDefault(c => c.Id == oldId)?.Name ?? diff.OldValue;
                                }
                                if (int.TryParse(diff.NewValue, out int newId))
                                {
                                    diff.NewValue = Categories.FirstOrDefault(c => c.Id == newId)?.Name ?? diff.NewValue;
                                }
                            }
                        }

                        await _dialogService.ShowRevisionDiffAsync(diffs);
                        return;
                    }
                }
                catch
                {
                    // Fallback
                }

                await _dialogService.ShowMessageAsync(revision.Content, $"Revision - {revision.ModifiedAt:g}");
            }
        }
    }
}

