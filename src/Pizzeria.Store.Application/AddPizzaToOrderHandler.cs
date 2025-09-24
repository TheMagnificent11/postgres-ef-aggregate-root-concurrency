using Microsoft.EntityFrameworkCore;
using Pizzeria.Store.Data;
using Pizzeria.Store.Domain;

namespace Pizzeria.Store.Application;

public class AddPizzaToOrderHandler<TDbContext>
    where TDbContext : IStoreDbContext
{
    public static async Task HandleAsync(
        Guid orderId,
        Guid pizzaId,
        TDbContext db,
        CancellationToken cancellationToken)
    {
        var pizza = Menu.Pizzas.FirstOrDefault(x => x.Id == pizzaId);
        if (pizza is null)
        {
            throw new ArgumentException("Invalid pizza ID.", nameof(pizzaId));
        }

        var order = await db.Orders
            .Include(x => x.Pizzas)
            .FirstOrDefaultAsync(x => x.Id == orderId, cancellationToken);
        if (order is null)
        {
            throw new ArgumentException("Order not found.", nameof(orderId));
        }

        order.AddPizza(pizza);

        await db.SaveChangesAsync(cancellationToken);
    }
}