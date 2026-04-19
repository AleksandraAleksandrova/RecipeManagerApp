using System.Collections.Generic;
using System.Threading.Tasks;

namespace RecipeManager.Core.Interfaces
{
    public interface IDialogService
    {
        Task<bool> ShowConfirmDeleteAsync(string message);
        Task ShowMessageAsync(string message, string title = "Message");
        Task ShowRevisionDiffAsync(object diffs);
    }
}
