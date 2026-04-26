using System;
using System.Collections.Generic;

namespace RecipeManager.Core.Models
{
    public class Category
    {
        public Category()
        {
            Recipes = new List<Recipe>();
        }

        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string CommonIngredients { get; set; } = string.Empty;

        public virtual ICollection<Recipe> Recipes { get; set; }
    }
}
