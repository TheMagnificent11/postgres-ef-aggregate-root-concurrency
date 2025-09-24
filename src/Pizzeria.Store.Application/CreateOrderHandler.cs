using Pizzeria.Store.Data;
using Pizzeria.Store.Domain;

namespace Pizzeria.Store.Application;

public class CreateOrderHandler<TDbContext>
    where TDbContext : IStoreDbContext
{
    public static async Task HandleAsync(
        TDbContext db,
        CancellationToken cancellationToken)
    {
        var order = Order.StartNewOrder(userId: "todo");

        db.Orders.Add(order);

        await db.SaveChangesAsync(cancellationToken);
    }
}