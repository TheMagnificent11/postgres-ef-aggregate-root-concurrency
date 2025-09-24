using Microsoft.EntityFrameworkCore;
using Pizzeria.Store.Domain;

namespace Pizzeria.Store.Data;

public interface IStoreDbContext
{
    DbSet<Pizza> Pizzas { get; }
    
    DbSet<Order> Orders { get; }
    
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}