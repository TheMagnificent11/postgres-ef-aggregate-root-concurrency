using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Pizzeria.Store.Data;
using Pizzeria.Store.Domain;

namespace Pizzeria.Store.Application;

public class AddPizzaToOrderHandler<TDbContext>
    where TDbContext : IStoreDbContext
{
    public static async Task<IResult> HandleAsync(
        Guid orderId,
        Guid pizzaId,
        TDbContext db,
        ILogger logger,
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