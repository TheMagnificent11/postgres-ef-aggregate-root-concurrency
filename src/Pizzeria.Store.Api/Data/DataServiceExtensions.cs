using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Pizzeria.Store.Data;

namespace Pizzeria.Store.Api.Data;

public static class DataServiceExtensions
{
    public static IServiceCollection AddPostgres<T>(
        this IServiceCollection services,
        string connectionString,
        string? schema,
        bool isDevelopment = false)
        where T : DbContext
    {
        services
            .AddDbContextFactory<T>((provider, options) =>
            {
                options.UseNpgsql(
                    connectionString,
                    x => x.MigrationsHistoryTable(HistoryRepository.DefaultTableName, schema));

                options.AddInterceptors(new AuditDetailsSaveChangesInterceptor());

                if (isDevelopment)
                {
                    options.EnableSensitiveDataLogging();
                    options.EnableDetailedErrors();

                    var logger = provider.GetRequiredService<ILogger<T>>();

                    options.LogTo(
                        message =>
                        {
                            if (message.Contains("Executing DbCommand"))
                            {
                                logger.LogInformation($"[EF SQL] {{SQL}}", message);
                            }
                        },
                        [DbLoggerCategory.Database.Command.Name],
                        LogLevel.Information);
                }
            });

        return services;
    }

    public static IServiceCollection AddSqlServer<T>(
        this IServiceCollection services,
        string connectionString,
        string? schema,
        bool isDevelopment = false)
        where T : DbContext
    {
        services
            .AddDbContextFactory<T>((provider, options) =>
            {
                options.UseSqlServer(
                    connectionString,
                    x => x.MigrationsHistoryTable(HistoryRepository.DefaultTableName, schema));

                options.AddInterceptors(new AuditDetailsSaveChangesInterceptor());

                if (isDevelopment)
                {
                    options.EnableSensitiveDataLogging();
                    options.EnableDetailedErrors();

                    var logger = provider.GetRequiredService<ILogger<T>>();

                    options.LogTo(
                        message =>
                        {
                            if (message.Contains("Executing DbCommand"))
                            {
                                logger.LogInformation($"[EF SQL] {{SQL}}", message);
                            }
                        },
                        [DbLoggerCategory.Database.Command.Name],
                        LogLevel.Information);
                }
            });

        return services;
    }

    public static IServiceCollection AddDatabaseSeeder<TContext, TSeeder>(
        this IServiceCollection services)
        where TContext : DbContext
        where TSeeder : class, Pizzeria.Store.Data.IDatabaseSeeder<TContext>
    {
        return services.AddScoped<Pizzeria.Store.Data.IDatabaseSeeder<TContext>, TSeeder>();
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

            var seeder = serviceScope.ServiceProvider.GetService<Pizzeria.Store.Data.IDatabaseSeeder<T>>();
            if (seeder == null)
            {
                return;
            }

            await seeder.RunAsync(cancellationTokenSource.Token);
        }
    }
}
