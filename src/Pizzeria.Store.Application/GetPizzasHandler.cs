using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Pizzeria.Store.Data;
using Pizzeria.Store.Domain;

namespace Pizzeria.Store.Application;

public class GetPizzasHandler<TDbContext>
    where TDbContext : IStoreDbContext
{
    public static async Task<IResult> HandleAsync(
        TDbContext db,
        ILogger logger,
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