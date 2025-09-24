using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Pizzeria.Store.Domain;

namespace Pizzeria.Store.Data;

public static class EntityEntryExtensions
{
    public static bool HasChangedOwnedEntities(this EntityEntry entry) =>
        entry.References.Any(r =>
            r.TargetEntry != null &&
            r.TargetEntry.Metadata.IsOwned() &&
            (r.TargetEntry.State == EntityState.Added || r.TargetEntry.State == EntityState.Modified));

    public static bool IsAggregateRoot(this EntityEntry entry) =>
        entry.Entity.GetType().IsSubclassOf(typeof(AggregateRoot));
}