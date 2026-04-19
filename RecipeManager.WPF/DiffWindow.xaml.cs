using System.Collections.Generic;
using System.Windows;
using System.Linq;

using RecipeManager.Core.Models;

namespace RecipeManager.WPF
{
    public partial class DiffWindow : Window
    {
        public DiffWindow(IEnumerable<DiffItem> differences)
        {
            InitializeComponent();
            DiffItemsControl.ItemsSource = differences.ToList();
        }
    }
}
