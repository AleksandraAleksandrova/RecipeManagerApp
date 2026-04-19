using System.Threading.Tasks;
using Microsoft.JSInterop;
using RecipeManager.Core.Interfaces;

namespace RecipeManager.Blazor.Services
{
    public class BlazorDialogService : IDialogService
    {
        private readonly IJSRuntime _jsRuntime;

        public BlazorDialogService(IJSRuntime jsRuntime)
        {
            _jsRuntime = jsRuntime;
        }

        public async Task<bool> ShowConfirmDeleteAsync(string message)
        {
            return await _jsRuntime.InvokeAsync<bool>("confirm", message);
        }

        public async Task ShowMessageAsync(string message, string title = "Message")
        {
            await _jsRuntime.InvokeVoidAsync("alert", $"{title}\n\n{message}");
        }

        public async Task ShowRevisionDiffAsync(object diffs)
        {
            // Simple stringification for blazor JS alert
            var json = System.Text.Json.JsonSerializer.Serialize(diffs, new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
            await _jsRuntime.InvokeVoidAsync("alert", $"Revision Differences:\n\n{json}");
        }
    }
}