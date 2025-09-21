using Microsoft.EntityFrameworkCore;
using Pizzeria.Common;
using Pizzeria.ServiceDefaults;
using Pizzeria.Store.Api.Data;
using Pizzeria.Store.Api.Domain;

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

app.MapGet(
    Endpoints.StoreApi.Pizzas,
    async (StoreDbContext db, CancellationToken cancellationToken) =>
    {
        var pizzas = await db.Pizzas.ToListAsync(cancellationToken);

        return Results.Ok(pizzas);
    });

app.MapPost(
    Endpoints.StoreApi.Orders,
    async (StoreDbContext db, CancellationToken cancellationToken) =>
    {
        var order = Order.StartNewOrder(userId: "todo");

        db.Orders.Add(order);

        await db.SaveChangesAsync(cancellationToken);

        return Results.Ok();
    });

app.MapPut(
    Endpoints.StoreApi.AddPizzaToOrder,
    async (Guid orderId, Guid pizzaId, StoreDbContext db, CancellationToken cancellationToken) =>
    {
        var pizza = Menu.Pizzas.FirstOrDefault(x => x.Id == pizzaId);
        if (pizza is null)
        {
            return Results.BadRequest("Invalid pizza ID.");
        }

        var order = await db.Orders
            .Include(x => x.Pizzas)
            .FirstOrDefaultAsync(x => x.Id == orderId, cancellationToken);
        if (order is null)
        {
            return Results.NotFound("Order not found.");
        }

        order.AddPizza(pizza);

        await db.SaveChangesAsync(cancellationToken);

        return Results.Ok();
    });

await app.Services.MigrateDatabaseAsync<StoreDbContext>(seedData: true);

await app.RunAsync();
