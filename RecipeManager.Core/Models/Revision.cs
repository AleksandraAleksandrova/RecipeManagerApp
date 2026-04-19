using System;
using System.ComponentModel.DataAnnotations;

namespace RecipeManager.Core.Models
{
    public class Revision
    {
        public int Id { get; set; }

        public int RecipeId { get; set; }

        [Required]
        public string Content { get; set; } = string.Empty;

        public DateTime ModifiedAt { get; set; }

        [Required]
        [StringLength(100)]
        public string ModifiedBy { get; set; } = string.Empty;

        public virtual Recipe Recipe { get; set; } = null!;
    }
}
