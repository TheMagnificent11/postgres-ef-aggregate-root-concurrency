using Microsoft.EntityFrameworkCore;
using Pizzeria.Store.Api.Data;
using Pizzeria.Store.Api.Domain;

namespace Pizzeria.Store.Api.Handlers;

public class AddPizzaToOrderHandler
{
    public static async Task<IResult> HandleAsync(
        Guid orderId,
        Guid pizzaId,
        StoreDbContext db,
        ILogger<AddPizzaToOrderHandler> logger,
        CancellationToken cancellationToken)
    {
        try
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
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while adding pizza {PizzaId} to order {OrderId}", pizzaId, orderId);
            return Results.Problem("An error occurred while processing your request.");
        }
    }
}