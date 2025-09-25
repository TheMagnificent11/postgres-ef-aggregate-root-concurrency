using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pizzeria.Store.Domain;

namespace Pizzeria.Store.Data.Configuration;

public sealed class OrderPizzaConfiguration : IEntityTypeConfiguration<OrderPizza>
{
    public void Configure(EntityTypeBuilder<OrderPizza> builder)
    {
        builder.HasKey(x => x.Id);
        
        // Since we're manually setting the ID, we need to configure it properly
        builder.Property(x => x.Id)
            .ValueGeneratedNever();

        builder
            .HasOne(op => op.Order)
            .WithMany(o => o.Pizzas)
            .HasForeignKey(op => op.OrderId);

        builder
            .HasOne(op => op.Pizza)
            .WithMany(p => p.OrderPizzas)
            .HasForeignKey(op => op.PizzaId);
    }
}