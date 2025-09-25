using Microsoft.EntityFrameworkCore;
using Pizzeria.Common;
using Pizzeria.ServiceDefaults;
using Pizzeria.Store.Api.Data;
using Pizzeria.Store.Api.Postgres;
using Pizzeria.Store.Api.SqlServer;
using Pizzeria.Store.Application;
using Pizzeria.Store.Data;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// Configure PostgreSQL database
builder.Services
    .AddPostgres<StorePostgresDbContext>(
        builder.Configuration.GetConnectionString(ServiceNames.PizzaStorePostgresDatabase)!,
        StorePostgresDbContext.SchemaName,
        builder.Environment.IsDevelopment())
    .AddDatabaseSeeder<StorePostgresDbContext, StoreSeeder<StorePostgresDbContext>>();

// Configure SQL Server database  
builder.Services
    .AddSqlServer<StoreSqlServerDbContext>(
        builder.Configuration.GetConnectionString(ServiceNames.PizzaStoreSqlServerDatabase)!,
        StoreSqlServerDbContext.SchemaName,
        builder.Environment.IsDevelopment())
    .AddDatabaseSeeder<StoreSqlServerDbContext, StoreSeeder<StoreSqlServerDbContext>>();

var app = builder.Build();

app.UseHttpsRedirection();
app.MapDefaultEndpoints();
app.UseRouting();

// PostgreSQL endpoints
app.MapGet(Endpoints.PostgresStoreApi.Pizzas, async (StorePostgresDbContext db, ILogger<Program> logger, CancellationToken cancellationToken) =>
{
    try
    {
        var pizzas = await GetPizzasHandler<StorePostgresDbContext>.HandleAsync(db, cancellationToken);
        return Results.Ok(pizzas);
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "An error occurred while getting pizzas from PostgreSQL");
        return Results.Problem("An error occurred while processing your request.");
    }
});

app.MapPost(Endpoints.PostgresStoreApi.Orders, async (StorePostgresDbContext db, ILogger<Program> logger, CancellationToken cancellationToken) =>
{
    try
    {
        await CreateOrderHandler<StorePostgresDbContext>.HandleAsync(db, cancellationToken);
        return Results.Ok();
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "An error occurred while creating order in PostgreSQL");
        return Results.Problem("An error occurred while processing your request.");
    }
});

app.MapPut(Endpoints.PostgresStoreApi.AddPizzaToOrder, async (Guid orderId, Guid pizzaId, StorePostgresDbContext db, ILogger<Program> logger, CancellationToken cancellationToken) =>
{
    try
    {
        await AddPizzaToOrderHandler<StorePostgresDbContext>.HandleAsync(orderId, pizzaId, db, cancellationToken);
        return Results.Ok();
    }
    catch (ArgumentException ex)
    {
        logger.LogWarning(ex, "Invalid argument while adding pizza to order in PostgreSQL");
        return Results.BadRequest(ex.Message);
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "An error occurred while adding pizza to order in PostgreSQL");
        return Results.Problem("An error occurred while processing your request.");
    }
});

// SQL Server endpoints
app.MapGet(Endpoints.SqlServerStoreApi.Pizzas, async (StoreSqlServerDbContext db, ILogger<Program> logger, CancellationToken cancellationToken) =>
{
    try
    {
        var pizzas = await GetPizzasHandler<StoreSqlServerDbContext>.HandleAsync(db, cancellationToken);
        return Results.Ok(pizzas);
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "An error occurred while getting pizzas from SQL Server");
        return Results.Problem("An error occurred while processing your request.");
    }
});

app.MapPost(Endpoints.SqlServerStoreApi.Orders, async (StoreSqlServerDbContext db, ILogger<Program> logger, CancellationToken cancellationToken) =>
{
    try
    {
        await CreateOrderHandler<StoreSqlServerDbContext>.HandleAsync(db, cancellationToken);
        return Results.Ok();
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "An error occurred while creating order in SQL Server");
        return Results.Problem("An error occurred while processing your request.");
    }
});

app.MapPut(Endpoints.SqlServerStoreApi.AddPizzaToOrder, async (Guid orderId, Guid pizzaId, StoreSqlServerDbContext db, ILogger<Program> logger, CancellationToken cancellationToken) =>
{
    try
    {
        await AddPizzaToOrderHandler<StoreSqlServerDbContext>.HandleAsync(orderId, pizzaId, db, cancellationToken);
        return Results.Ok();
    }
    catch (ArgumentException ex)
    {
        logger.LogWarning(ex, "Invalid argument while adding pizza to order in SQL Server");
        return Results.BadRequest(ex.Message);
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "An error occurred while adding pizza to order in SQL Server");
        return Results.Problem("An error occurred while processing your request.");
    }
});

// Migrate and seed both databases
await app.Services.MigrateDatabaseAsync<StorePostgresDbContext>(seedData: true);
await app.Services.MigrateDatabaseAsync<StoreSqlServerDbContext>(seedData: true);

await app.RunAsync();
