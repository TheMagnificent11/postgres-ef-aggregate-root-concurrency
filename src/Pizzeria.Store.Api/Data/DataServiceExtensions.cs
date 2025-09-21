using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Pizzeria.Store.Api.Data;

public static class DataServiceExtensions
{
    public static IServiceCollection AddPostgres<T>(
        this IServiceCollection services,
        string connectionString,
        string? schema)
        where T : DbContext
    {
        services
            .AddDbContextFactory<T>((provider, options) =>
            {
                options.UseNpgsql(
                    connectionString,
                    x => x.MigrationsHistoryTable(HistoryRepository.DefaultTableName, schema));

                options.AddInterceptors(new AuditDetailsSaveChangesInterceptor());
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
