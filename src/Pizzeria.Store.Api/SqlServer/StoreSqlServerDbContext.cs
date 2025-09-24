using Microsoft.EntityFrameworkCore;
using Pizzeria.Store.Data;
using Pizzeria.Store.Domain;

namespace Pizzeria.Store.Api.SqlServer;

public sealed class StoreSqlServerDbContext : DbContext, IStoreDbContext
{
    public const string SchemaName = "sto";

    public StoreSqlServerDbContext(DbContextOptions<StoreSqlServerDbContext> options)
        : base(options)
    {
    }

    public DbSet<Pizza> Pizzas { get; set; }

    public DbSet<Order> Orders { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.HasDefaultSchema(SchemaName);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(IStoreDbContext).Assembly);
    }
}