using Pizzeria.Common;
using Pizzeria.ServiceDefaults;
using Pizzeria.Store.Api.Data;
using Pizzeria.Store.Api.Handlers;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services
    .AddPostgres<StoreDbContext>(
        builder.Configuration.GetConnectionString(ServiceNames.PizzaStoreDatabase)!,
        StoreDbContext.SchemaName)
    .AddDatabaseSeeder<StoreDbContext, StoreSeeder>();

var app = builder.Build();

app.UseHttpsRedirection();
app.MapDefaultEndpoints();
app.UseRouting();

app.MapGet(Endpoints.StoreApi.Pizzas, GetPizzasHandler.HandleAsync);
app.MapPost(Endpoints.StoreApi.Orders, CreateOrderHandler.HandleAsync);
app.MapPut(Endpoints.StoreApi.AddPizzaToOrder, AddPizzaToOrderHandler.HandleAsync);

await app.Services.MigrateDatabaseAsync<StoreDbContext>(seedData: true);

await app.RunAsync();
