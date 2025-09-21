using Microsoft.EntityFrameworkCore;

namespace Pizzeria.Store.Api.Data;

public interface IDatabaseSeeder<TDbContext>
    where TDbContext : DbContext
{
    Task RunAsync(CancellationToken cancellationToken = default);
}
