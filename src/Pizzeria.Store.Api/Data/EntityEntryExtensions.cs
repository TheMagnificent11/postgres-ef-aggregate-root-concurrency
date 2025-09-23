using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Pizzeria.Store.Api.Domain;

namespace Pizzeria.Store.Api.Data;

public static class EntityEntryExtensions
{
    public static bool HasChangedOwnedEntities(this EntityEntry entry) =>
        entry.References.Any(r =>
            r.TargetEntry != null &&
            r.TargetEntry.Metadata.IsOwned() &&
            (r.TargetEntry.State == EntityState.Added || r.TargetEntry.State == EntityState.Modified));

    public static bool HasChangedRelatedEntities(this EntityEntry entry) =>
        entry.Collections.Any(c =>
            c.CurrentValue != null &&
            c.CurrentValue.Cast<object>().Any(related => 
            {
                var relatedEntry = entry.Context.Entry(related);
                return relatedEntry.State == EntityState.Added || 
                       relatedEntry.State == EntityState.Modified || 
                       relatedEntry.State == EntityState.Deleted;
            }));

    public static bool IsAggregateRoot(this EntityEntry entry) =>
        entry.Entity.GetType().IsSubclassOf(typeof(AggregateRoot));
}
