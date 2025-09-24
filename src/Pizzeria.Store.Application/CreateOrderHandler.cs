using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Pizzeria.Store.Data;
using Pizzeria.Store.Domain;

namespace Pizzeria.Store.Application;

public class CreateOrderHandler<TDbContext>
    where TDbContext : IStoreDbContext
{
    public static async Task<IResult> HandleAsync(
        TDbContext db,
        ILogger<CreateOrderHandler<TDbContext>> logger,
        CancellationToken cancellationToken)
    {
        try
        {
            var order = Order.StartNewOrder(userId: "todo");

            db.Orders.Add(order);

            await db.SaveChangesAsync(cancellationToken);

            return Results.Ok();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while creating a new order");
            return Results.Problem("An error occurred while processing your request.");
        }
    }
}