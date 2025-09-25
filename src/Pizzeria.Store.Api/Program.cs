using Microsoft.EntityFrameworkCore;
using Pizzeria.Common;
using Pizzeria.ServiceDefaults;
using Pizzeria.Store.Api.Data;
using Pizzeria.Store.Api.Postgres;
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
//builder.Services
//    .AddSqlServer<StoreSqlServerDbContext>(
//        builder.Configuration.GetConnectionString(ServiceNames.PizzaStoreSqlServerDatabase)!,
//        StoreSqlServerDbContext.SchemaName,
//        builder.Environment.IsDevelopment())
//    .AddDatabaseSeeder<StoreSqlServerDbContext, StoreSeeder<StoreSqlServerDbContext>>();

var app = builder.Build();

app.UseHttpsRedirection();
app.MapDefaultEndpoints();
app.UseRouting();

// PostgreSQL endpoints
app.MapGet(Endpoints.PostgresStoreApi.Pizzas, async (StorePostgresDbContext db, ILogger<Program> logger, CancellationToken cancellationToken) =>
    await GetPizzasHandler<StorePostgresDbContext>.HandleAsync(db, logger, cancellationToken));

app.MapPost(Endpoints.PostgresStoreApi.Orders, async (StorePostgresDbContext db, ILogger<Program> logger, CancellationToken cancellationToken) =>
    await CreateOrderHandler<StorePostgresDbContext>.HandleAsync(db, logger, cancellationToken));

app.MapPut(Endpoints.PostgresStoreApi.AddPizzaToOrder, async (Guid orderId, Guid pizzaId, StorePostgresDbContext db, ILogger<Program> logger, CancellationToken cancellationToken) =>
    await AddPizzaToOrderHandler<StorePostgresDbContext>.HandleAsync(orderId, pizzaId, db, logger, cancellationToken));

// SQL Server endpoints
//app.MapGet(Endpoints.SqlServerStoreApi.Pizzas, async (StoreSqlServerDbContext db, ILogger<Program> logger, CancellationToken cancellationToken) =>
//    await GetPizzasHandler<StoreSqlServerDbContext>.HandleAsync(db, logger, cancellationToken));

//app.MapPost(Endpoints.SqlServerStoreApi.Orders, async (StoreSqlServerDbContext db, ILogger<Program> logger, CancellationToken cancellationToken) =>
//    await CreateOrderHandler<StoreSqlServerDbContext>.HandleAsync(db, logger, cancellationToken));

//app.MapPut(Endpoints.SqlServerStoreApi.AddPizzaToOrder, async (Guid orderId, Guid pizzaId, StoreSqlServerDbContext db, ILogger<Program> logger, CancellationToken cancellationToken) =>
//    await AddPizzaToOrderHandler<StoreSqlServerDbContext>.HandleAsync(orderId, pizzaId, db, logger, cancellationToken));

// Migrate and seed both databases
await app.Services.MigrateDatabaseAsync<StorePostgresDbContext>(seedData: true);
//await app.Services.MigrateDatabaseAsync<StoreSqlServerDbContext>(seedData: true);

await app.RunAsync();
