using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pizzeria.Store.Api.Domain;

namespace Pizzeria.Store.Api.Data.Configuration;

public abstract class AggregateRootConfiguration<T> : EntityConfiguration<T>
    where T : AggregateRoot
{
    /// <inheritdoc/>
    public override void Configure(EntityTypeBuilder<T> builder)
    {
        base.Configure(builder);

        builder.Property(x => x.Version)
            .IsRowVersion();
    }
}
