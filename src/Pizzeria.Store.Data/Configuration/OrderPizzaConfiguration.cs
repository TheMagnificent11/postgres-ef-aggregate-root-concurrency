using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pizzeria.Store.Domain;

namespace Pizzeria.Store.Data.Configuration;

public sealed class OrderPizzaConfiguration : IEntityTypeConfiguration<OrderPizza>
{
    public void Configure(EntityTypeBuilder<OrderPizza> builder)
    {
        builder.HasKey(x => x.Id);
        
        builder.Property(x => x.Id)
            .ValueGeneratedOnAdd();

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