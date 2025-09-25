using Microsoft.EntityFrameworkCore;

namespace Pizzeria.Store.Data;

public interface IDatabaseSeeder<TDbContext>
    where TDbContext : DbContext
{
    Task RunAsync(CancellationToken cancellationToken = default);
}