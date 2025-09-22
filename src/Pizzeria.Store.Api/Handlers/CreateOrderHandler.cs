using Pizzeria.Store.Api.Data;
using Pizzeria.Store.Api.Domain;

namespace Pizzeria.Store.Api.Handlers;

public class CreateOrderHandler
{
    public static async Task<IResult> HandleAsync(
        StoreDbContext db,
        ILogger<CreateOrderHandler> logger,
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
