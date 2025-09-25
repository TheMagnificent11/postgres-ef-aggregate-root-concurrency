using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Pizzeria.Store.Domain;

namespace Pizzeria.Store.Data;

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

            if (entry.State == EntityState.Unchanged && entry.IsAggregateRoot())
            {
                entry.Entity.ApplyModificationTrackingData(modifiedBy: "todo");
            }
        }
    }
}