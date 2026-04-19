using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace RecipeManager.Core.Models
{
    public class Recipe
    {
        public Recipe()
        {
            Revisions = new List<Revision>();
        }

        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Title { get; set; } = string.Empty;

        [StringLength(1000)]
        public string Description { get; set; } = string.Empty;

        [Required]
        public string Ingredients { get; set; } = string.Empty;

        [Required]
        public string Instructions { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }

        public virtual ICollection<Revision> Revisions { get; set; }

        public int? CategoryId { get; set; }
        public virtual Category Category { get; set; }
    }
}
