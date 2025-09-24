using Microsoft.EntityFrameworkCore;
using Pizzeria.Store.Data;
using Pizzeria.Store.Domain;

namespace Pizzeria.Store.Application;

public class GetPizzasHandler<TDbContext>
    where TDbContext : IStoreDbContext
{
    public static async Task<List<Pizza>> HandleAsync(
        TDbContext db,
        CancellationToken cancellationToken)
    {
        var pizzas = await db.Pizzas.ToListAsync(cancellationToken);
        return pizzas;
    }
}