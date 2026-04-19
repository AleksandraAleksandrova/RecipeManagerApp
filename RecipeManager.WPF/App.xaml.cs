using System;
using System.IO;
using System.Windows;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RecipeManager.Core.Interfaces;
using RecipeManager.Core.Services;
using RecipeManager.Infrastructure.Data;
using RecipeManager.Infrastructure.Repositories;
using RecipeManager.Core.ViewModels;
using RecipeManager.WPF.Services;

namespace RecipeManager.WPF
{
    public partial class App : Application
    {
        private IServiceProvider _serviceProvider;

        public App()
        {
            var services = new ServiceCollection();
            ConfigureServices(services);
            _serviceProvider = services.BuildServiceProvider();
        }

        private void ConfigureServices(IServiceCollection services)
        {
            services.AddLogging();

            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .Build();

            string dbProvider = configuration["DatabaseProvider"] ?? "SQLite";

            if (dbProvider.Equals("SqlServer", StringComparison.OrdinalIgnoreCase))
            {
                string sqlServerConn = configuration.GetConnectionString("SqlServer");
                services.AddDbContext<AppDbContext>(options => 
                {
                    options.UseSqlServer(sqlServerConn);
                    options.ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning));
                });
            }
            else
            {
                string sqliteConn = configuration.GetConnectionString("Sqlite");
                if (string.IsNullOrEmpty(sqliteConn) || sqliteConn == "Data Source=recipes.db")
                {
                    var dbFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "RecipeManager");
                    Directory.CreateDirectory(dbFolder);
                    string dbPath = Path.Combine(dbFolder, "recipes.db");
                    sqliteConn = $"Data Source={dbPath}";
                    System.Diagnostics.Debug.WriteLine($"WPF Using Shared DB Path: {dbPath}");
                }
                services.AddDbContext<AppDbContext>(options => 
                {
                    options.UseSqlite(sqliteConn);
                    options.ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning));
                });
            }

            // Core
            services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
            services.AddScoped<IRecipeRepository, RecipeRepository>();
            services.AddScoped<ICategoryRepository, CategoryRepository>();
            services.AddScoped<RecipeService>();
            services.AddScoped<CategoryService>();
            services.AddSingleton<IDialogService, WpfDialogService>();

            // ViewModels
            services.AddTransient<MainViewModel>();

            // Views
            services.AddTransient<MainWindow>();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            using (var scope = _serviceProvider.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                try
                {
                    dbContext.Database.Migrate();
                }
                catch (Exception ex)
                {
                    // Ignore migration issues when switching DBs with mismatched histories
                    System.Diagnostics.Debug.WriteLine($"Migration skipped/failed: {ex.Message}");
                }
            }

            var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
            mainWindow.Show();
        }
    }
}
