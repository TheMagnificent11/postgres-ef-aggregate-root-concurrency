using Microsoft.EntityFrameworkCore;
using Pizzeria.Store.Data;
using Pizzeria.Store.Domain;

namespace Pizzeria.Store.Api.Postgres;

public sealed class StorePostgresDbContext : DbContext, IStoreDbContext
{
    public const string SchemaName = "sto";

    public StorePostgresDbContext(DbContextOptions<StorePostgresDbContext> options)
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