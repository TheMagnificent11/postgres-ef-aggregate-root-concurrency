using Microsoft.EntityFrameworkCore;
using Pizzeria.Store.Api.Domain;

namespace Pizzeria.Store.Api.Data;

public sealed class StoreDbContext : DbContext
{
    public const string SchemaName = "sto";

    public StoreDbContext(DbContextOptions<StoreDbContext> options)
        : base(options)
    {
    }

    public DbSet<Pizza> Pizzas { get; set; }

    public DbSet<Order> Orders { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.HasDefaultSchema(SchemaName);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(StoreDbContext).Assembly);
    }
}
