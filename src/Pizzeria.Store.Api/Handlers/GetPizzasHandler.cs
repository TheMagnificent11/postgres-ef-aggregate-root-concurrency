using Microsoft.EntityFrameworkCore;
using Pizzeria.Store.Api.Data;

namespace Pizzeria.Store.Api.Handlers;

public class GetPizzasHandler
{
    public static async Task<IResult> HandleAsync(
        StoreDbContext db,
        ILogger<GetPizzasHandler> logger,
        CancellationToken cancellationToken)
    {
        try
        {
            var pizzas = await db.Pizzas.ToListAsync(cancellationToken);

            return Results.Ok(pizzas);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while getting pizzas");
            return Results.Problem("An error occurred while processing your request.");
        }
    }
}
