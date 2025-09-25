using Aspire.Hosting;
using Aspire.Hosting.ApplicationModel;
using Aspire.Hosting.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Pizzeria.Common;
using Pizzeria.Store.Api.Postgres;
using Pizzeria.Store.Api.SqlServer;
using Pizzeria.Store.Domain;
using Xunit;

namespace Pizzeria.Tests.Integration;

public sealed class PizzeriaApplicationFactory : IAsyncLifetime
{
    public const string CollectionName = "PizzeriaCollection";

    private IDistributedApplicationTestingBuilder builder;
    private DistributedApplication app;
    private ResourceNotificationService resourceNotificationService;
    private StorePostgresDbContext postgresDbContext;
    //private StoreSqlServerDbContext sqlServerDbContext;

    public async Task InitializeAsync()
    {
        this.builder = await DistributedApplicationTestingBuilder.CreateAsync<Projects.Pizzeria_AppHost>();
        this.builder.Services.ConfigureHttpClientDefaults(x =>
        {
            x.AddStandardResilienceHandler();
        });

        this.app = await this.builder.BuildAsync();
        this.resourceNotificationService = this.app.Services.GetRequiredService<ResourceNotificationService>();

        await this.app.StartAsync();

        await this.resourceNotificationService
            .WaitForResourceAsync(ServiceNames.PizzaStoreApi, KnownResourceStates.Running)
            .WaitAsync(TimeSpan.FromMinutes(10)); // 10 minutes because Docker images may need to be pulled

        // Setup PostgreSQL connection
        var postgresConnectionString = await this.app.GetConnectionStringAsync(ServiceNames.PizzaStorePostgresDatabase);
        var postgresOptionsBuilder = new DbContextOptionsBuilder<StorePostgresDbContext>();
        postgresOptionsBuilder.UseNpgsql(postgresConnectionString);
        this.postgresDbContext = new StorePostgresDbContext(postgresOptionsBuilder.Options);

        // Setup SQL Server connection
        //var sqlServerConnectionString = await this.app.GetConnectionStringAsync(ServiceNames.PizzaStoreSqlServerDatabase);
        //var sqlServerOptionsBuilder = new DbContextOptionsBuilder<StoreSqlServerDbContext>();
        //sqlServerOptionsBuilder.UseSqlServer(sqlServerConnectionString);
        //this.sqlServerDbContext = new StoreSqlServerDbContext(sqlServerOptionsBuilder.Options);
    }

    public async Task<HttpClient> GetServiceClientAsync(string serviceName)
    {
        var client = this.app.CreateHttpClient(serviceName);

        await this.resourceNotificationService
            .WaitForResourceAsync(serviceName, KnownResourceStates.Running)
            .WaitAsync(TimeSpan.FromSeconds(10));

        return client;
    }

    public async Task<Order> GetLatestPostgresOrder()
    {
        var order = await this.postgresDbContext
            .Orders
            .Include(x => x.Pizzas)
            .OrderByDescending(x => x.CreatedAtUtc)
            .FirstOrDefaultAsync();

        return order;
    }

    //public async Task<Order> GetLatestSqlServerOrder()
    //{
    //    var order = await this.sqlServerDbContext
    //        .Orders
    //        .Include(x => x.Pizzas)
    //        .OrderByDescending(x => x.CreatedAtUtc)
    //        .FirstOrDefaultAsync();

    //    return order;
    //}

    public async Task<Order> GetPostgresOrder(Guid orderId)
    {
        var order = await this.postgresDbContext
            .Orders
            .Include(x => x.Pizzas)
            .FirstOrDefaultAsync(x => x.Id == orderId);

        return order;
    }

    //public async Task<Order> GetSqlServerOrder(Guid orderId)
    //{
    //    var order = await this.sqlServerDbContext
    //        .Orders
    //        .Include(x => x.Pizzas)
    //        .FirstOrDefaultAsync(x => x.Id == orderId);

    //    return order;
    //}

    public async Task DisposeAsync()
    {
        if (this.postgresDbContext != null)
        {
            await this.postgresDbContext.DisposeAsync();
        }

        //if (this.sqlServerDbContext != null)
        //{
        //    await this.sqlServerDbContext.DisposeAsync();
        //}

        if (this.app != null)
        {
            await this.app.StopAsync();
            await this.app.DisposeAsync();
        }

        if (this.builder != null)
        {
            await this.builder.DisposeAsync();
        }
    }
}
