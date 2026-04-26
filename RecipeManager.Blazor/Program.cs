using RecipeManager.Blazor.Components;
using Microsoft.EntityFrameworkCore;
using RecipeManager.Infrastructure.Data;
using RecipeManager.Core.Interfaces;
using RecipeManager.Core.Services;
using RecipeManager.Infrastructure.Repositories;
using RecipeManager.Blazor.Services;
using RecipeManager.Core.ViewModels;
using MudBlazor.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddMudServices();

var dbProvider = builder.Configuration["DatabaseProvider"] ?? "SQLite";

if (dbProvider.Equals("SqlServer", StringComparison.OrdinalIgnoreCase))
{
    var sqlConn = builder.Configuration.GetConnectionString("SqlServer");
    builder.Services.AddDbContext<AppDbContext>(options =>
    {
        options.UseSqlServer(sqlConn);
        options.ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning));
    });
}
else
{
    var sqliteConn = builder.Configuration.GetConnectionString("Sqlite");
    if (string.IsNullOrEmpty(sqliteConn) || sqliteConn == "Data Source=recipes.db")
    {
        var dbFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "RecipeManager");
        Directory.CreateDirectory(dbFolder);
        var dbPath = Path.Combine(dbFolder, "recipes.db");
        sqliteConn = $"Data Source={dbPath}";
        System.Diagnostics.Debug.WriteLine($"Blazor Using Shared DB Path: {dbPath}");
    }
    builder.Services.AddDbContext<AppDbContext>(options =>
    {
        options.UseSqlite(sqliteConn);
        options.ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning));
    });
}

builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
builder.Services.AddScoped<IRecipeRepository, RecipeRepository>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<RecipeService>();
builder.Services.AddScoped<CategoryService>();
builder.Services.AddScoped<IDialogService, BlazorDialogService>();
builder.Services.AddScoped<MainViewModel>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    try
    {
        dbContext.Database.Migrate();
    }
    catch (Exception ex)
    {
        // Gracefully ignore if the database already has the schema but misses the exact EF history,
        // which happens when switching providers and recreating migrations.
        app.Logger.LogWarning($"Migration skipped/failed: {ex.Message}");
    }
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
