using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pizzeria.Store.Domain;

namespace Pizzeria.Store.Data.Configuration;

public abstract class AggregateRootConfiguration<T> : EntityConfiguration<T>
    where T : AggregateRoot
{
    /// <inheritdoc/>
    public override void Configure(EntityTypeBuilder<T> builder)
    {
        base.Configure(builder);

        // Note: Version property removed as per requirements - no concurrency tokens
    }
}