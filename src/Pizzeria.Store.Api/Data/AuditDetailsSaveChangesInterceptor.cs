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

            if (entry.State == EntityState.Unchanged && entry.IsAggregateRoot())
            {
                // Check if this aggregate root has any child entities that are added, modified, or deleted
                var hasChangedChildren = context.ChangeTracker.Entries()
                    .Where(e => e.State != EntityState.Unchanged)
                    .Any(e => HasForeignKeyTo(e, entry));

                if (hasChangedChildren)
                {
                    entry.Entity.ApplyModificationTrackingData(modifiedBy: "todo");
                    // Mark the aggregate root as modified so PostgreSQL will update the xmin concurrency token
                    entry.State = EntityState.Modified;
                }
            }
        }
    }

    private static bool HasForeignKeyTo(EntityEntry childEntry, EntityEntry parentEntry)
    {
        // Check if the child entity has any foreign key properties that reference the parent
        foreach (var foreignKey in childEntry.Metadata.GetForeignKeys())
        {
            var principalType = foreignKey.PrincipalEntityType.ClrType;
            if (principalType == parentEntry.Entity.GetType())
            {
                // Get the foreign key value from the child
                var foreignKeyProperties = foreignKey.Properties;
                var principalKeyProperties = foreignKey.PrincipalKey.Properties;
                
                // Compare the key values
                for (int i = 0; i < foreignKeyProperties.Count; i++)
                {
                    var childKeyValue = childEntry.Property(foreignKeyProperties[i].Name).CurrentValue;
                    var parentKeyValue = parentEntry.Property(principalKeyProperties[i].Name).CurrentValue;
                    
                    if (Equals(childKeyValue, parentKeyValue))
                    {
                        return true;
                    }
                }
            }
        }

        return false;
    }
}
