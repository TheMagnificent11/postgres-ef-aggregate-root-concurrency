using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Pizzeria.Store.Api.Domain;

namespace Pizzeria.Store.Api.Data;

public sealed class AuditDetailsSaveChangesInterceptor : SaveChangesInterceptor
{
    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        UpdateEntities(eventData.Context);

        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        UpdateEntities(eventData.Context);

        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private static void UpdateEntities(DbContext? context)
    {
        if (context == null)
        {
            return;
        }

        foreach (var entry in context.ChangeTracker.Entries<Entity>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.ApplyCreationTrackingData(createdBy: "todo");
            }

            if (entry.State == EntityState.Added ||
                entry.State == EntityState.Modified ||
                entry.HasChangedOwnedEntities())
            {
                entry.Entity.ApplyModificationTrackingData(modifiedBy: "todo");
            }

            // Handle aggregate root concurrency when children change
            if (entry.State == EntityState.Unchanged && entry.IsAggregateRoot())
            {
                if (entry.HasChangedRelatedEntities())
                {
                    entry.Entity.ApplyModificationTrackingData(modifiedBy: "todo");
                    // Mark the aggregate root as modified so PostgreSQL will update the xmin concurrency token
                    entry.State = EntityState.Modified;
                }
            }
        }
    }

}
