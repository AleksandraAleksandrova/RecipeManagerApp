using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using RecipeManager.Core.Interfaces;
using RecipeManager.Core.Models;

namespace RecipeManager.WPF.Services
{
    public class WpfDialogService : IDialogService
    {
        public Task<bool> ShowConfirmDeleteAsync(string message)
        {
            var deleteWindow = new DeleteConfirmWindow(message);
            deleteWindow.Owner = Application.Current.MainWindow;
            deleteWindow.ShowDialog();
            return Task.FromResult(deleteWindow.IsConfirmed);
        }

        public Task ShowMessageAsync(string message, string title = "Message")
        {
            MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Information);
            return Task.CompletedTask;
        }

        public Task ShowRevisionDiffAsync(object diffs)
        {
            if (diffs is IEnumerable<DiffItem> d)
            {
                var window = new DiffWindow(d);
                window.Owner = Application.Current.MainWindow;
                window.ShowDialog();
            }
            return Task.CompletedTask;
        }
    }
}
