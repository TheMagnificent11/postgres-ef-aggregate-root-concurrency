using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Pizzeria.Store.Api.Data;

public static class DataServiceExtensions
{
    public static IServiceCollection AddPostgres<T>(
        this IServiceCollection services,
        string connectionString,
        string? schema,
        Action<string>? sqlLogger = null)
        where T : DbContext
    {
        services
            .AddDbContextFactory<T>((provider, options) =>
            {
                options.UseNpgsql(
                    connectionString,
                    x => x.MigrationsHistoryTable(HistoryRepository.DefaultTableName, schema));

                options.AddInterceptors(new AuditDetailsSaveChangesInterceptor());

                // Enable SQL logging - this will log to the configured logger
                options.EnableSensitiveDataLogging(); // Shows parameter values
                options.EnableDetailedErrors(); // Shows more detailed error information

                // Custom SQL logging with better formatting
                var logAction = sqlLogger ?? (message => Console.WriteLine($"[EF SQL] {DateTime.Now:HH:mm:ss.fff} - {message}"));
                
                options.LogTo(message =>
                {
                    if (message.Contains("Executing DbCommand"))
                    {
                        logAction(message);
                    }
                }, new[] { DbLoggerCategory.Database.Command.Name }, LogLevel.Information);
            });

        return services;
    }

    public static IServiceCollection AddDatabaseSeeder<TContext, TSeeder>(
        this IServiceCollection services)
        where TContext : DbContext
        where TSeeder : class, IDatabaseSeeder<TContext>
    {
        return services.AddScoped<IDatabaseSeeder<TContext>, TSeeder>();
    }

    public static async Task MigrateDatabaseAsync<T>(this IServiceProvider serviceProvider, bool seedData = false)
        where T : DbContext
    {
        using (var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromMinutes(5)))
        using (var serviceScope = serviceProvider.CreateScope())
        using (var dbContext = serviceScope.ServiceProvider.GetRequiredService<T>())
        {
            while (!cancellationTokenSource.IsCancellationRequested)
            {
                var canConnect = await dbContext.Database.CanConnectAsync(cancellationTokenSource.Token);
                if (canConnect)
                {
                    break;
                }

                await Task.Delay(TimeSpan.FromSeconds(5), cancellationTokenSource.Token);
            }

            if (cancellationTokenSource.IsCancellationRequested)
            {
                return;
            }

            await dbContext.Database.MigrateAsync(cancellationTokenSource.Token);

            if (!seedData)
            {
                return;
            }

            var seeder = serviceScope.ServiceProvider.GetService<IDatabaseSeeder<T>>();
            if (seeder == null)
            {
                return;
            }

            await seeder.RunAsync(cancellationTokenSource.Token);
        }
    }
}
