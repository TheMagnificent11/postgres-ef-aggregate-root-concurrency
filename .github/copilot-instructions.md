# Copilot Instructions for PostgreSQL EF Aggregate Root Concurrency

## Repository Overview

This repository demonstrates a concurrency issue with PostgreSQL Entity Framework Core when working with Domain-Driven Design aggregate roots. It's a minimal reproduction case for a pizzeria application that uses .NET Aspire for orchestration.

### Purpose
- Demonstrates PostgreSQL EF Core concurrency token behavior differences from SQL Server
- Shows how aggregate root concurrency tokens work (or fail) with child entity modifications
- Provides a test case for investigating and fixing the concurrency issue

## Architecture & Technology Stack

### Core Technologies
- **.NET 9** - Latest .NET framework
- **.NET Aspire** - Application orchestration and configuration
- **Entity Framework Core** - ORM with PostgreSQL provider
- **PostgreSQL** - Database with `xmin` concurrency tokens
- **xUnit** - Testing framework with Aspire integration testing
- **Docker** - Database containerization

### Project Structure
```
src/
├── Pizzeria.AppHost/           # .NET Aspire orchestrator
├── Pizzeria.ServiceDefaults/   # Shared Aspire service configuration  
├── Pizzeria.Common/           # Shared constants and utilities
└── Pizzeria.Store.Api/        # Main API with domain models and data layer
tests/
└── Pizzeria.Tests.Integration/ # Aspire-based integration tests
```

## Domain-Driven Design Patterns

### Entity Hierarchy
- **`Entity`** - Base class with audit fields (`CreatedBy`, `ModifiedBy`, etc.)
- **`AggregateRoot`** - Extends Entity, adds `Version` concurrency token
- **`Order`** - Aggregate root managing pizza orders
- **`Pizza`** - Aggregate root for menu items
- **`OrderPizza`** - Value object linking orders to pizzas

### Key Domain Concepts
- **Aggregate Boundaries** - All operations go through aggregate roots
- **Concurrency Control** - Uses `uint Version` property mapped to PostgreSQL `xmin`
- **Encapsulation** - Child entities accessed only through aggregate roots
- **Domain Events** - (Pattern present but not fully implemented)

## Development Workflows

### Prerequisites
```bash
# Required software
- .NET 9 SDK
- Docker Desktop
- PostgreSQL client tools (optional)
```

### Common Commands
```bash
# Build solution
dotnet build

# Run all tests (reproduces the concurrency issue)
dotnet test

# Run Aspire application
dotnet run --project src/Pizzeria.AppHost

# Generate EF migrations
dotnet ef migrations add <MigrationName> --project src/Pizzeria.Store.Api

# Update database
dotnet ef database update --project src/Pizzeria.Store.Api
```

### API Endpoints
The API uses minimal APIs defined in `Endpoints.StoreApi`:
- `GET /pizzas` - Retrieve menu items
- `POST /orders` - Create new order (works correctly)  
- `PUT /orders/{orderId}/pizzas/{pizzaId}` - Add pizza to order (fails with concurrency exception)

Route constants are defined in `Pizzeria.Common.Endpoints` for consistency across projects.

## The Concurrency Issue

### Problem Description
When adding a pizza to an existing order (child entity modification), PostgreSQL EF Core throws:
```
Microsoft.EntityFrameworkCore.DbUpdateConcurrencyException: 
The database operation was expected to affect 1 row(s), but actually affected 0 row(s)
```

### Root Cause
- PostgreSQL uses `xmin` system column for row versioning
- EF Core maps `AggregateRoot.Version` to `xmin` via `.IsRowVersion()`
- Child entity modifications don't properly update parent aggregate's version
- Leads to concurrency conflict during `SaveChanges()`

### Test Reproduction
```csharp
// This test passes - creating an order
[Fact] Should_CreateOrder_When_OrderIsPlaced()

// This test fails - adding pizza to existing order  
[Fact] Should_AddPizzaToOrder_When_PizzaIsAdded()
```

## Development Patterns & Guidelines

### Entity Configuration
```csharp
// Base configuration for all entities
public abstract class EntityConfiguration<TEntity> : IEntityTypeConfiguration<TEntity>
    where TEntity : Entity

// Aggregate root configuration adds row version
public abstract class AggregateRootConfiguration<T> : EntityConfiguration<T>
    where T : AggregateRoot
{
    builder.Property(x => x.Version).IsRowVersion(); // Maps to xmin
}
```

### Domain Model Patterns
```csharp
// Aggregate root encapsulation
public class Order : AggregateRoot
{
    private readonly List<OrderPizza> pizzas; // Private collection
    public IReadOnlyCollection<OrderPizza> Pizzas => this.pizzas; // Read-only access
    
    public void AddPizza(Pizza pizza) // Behavior through methods
    {
        // Business logic here
        this.pizzas.Add(OrderPizza.CreateForOrder(this, pizza));
    }
}
```

### Aspire Configuration
```csharp
// AppHost setup
var databaseServer = builder.AddPostgres(ServiceNames.DatabaseServer);
var pizzaStoreDatabase = databaseServer.AddDatabase(ServiceNames.PizzaStoreDatabase);

// Service registration with database dependency
builder.AddProject<Projects.Pizzeria_Store_Api>(ServiceNames.PizzaStoreApi)
    .WithReference(pizzaStoreDatabase)
    .WaitFor(pizzaStoreDatabase);
```

### EF Core Interceptors
The application uses `AuditDetailsSaveChangesInterceptor` to automatically populate audit fields:
```csharp
// Automatically sets CreatedBy, ModifiedBy, CreatedAtUtc, ModifiedAtUtc
// Special handling for aggregate roots to track changes to child entities
options.AddInterceptors(new AuditDetailsSaveChangesInterceptor());
```

Key behavior:
- **Added entities** - Sets creation and modification tracking
- **Modified entities** - Updates modification tracking  
- **Unchanged aggregate roots** - Still updates modification tracking if children changed
- **Owned entities** - Properly handles owned entity modifications

## Testing with Aspire

### Integration Testing Setup
```csharp
[Collection(PizzeriaApplicationFactory.CollectionName)]
public sealed class PizzaOrderingTests
{
    // Uses DistributedApplicationTestingBuilder for real containers
    // Automatically manages PostgreSQL container lifecycle
    // Provides HTTP clients for API testing
}
```

### Test Infrastructure
- **`PizzeriaApplicationFactory`** - Manages Aspire test application
- **Collection-based testing** - Shares container across tests for efficiency
- **Real PostgreSQL** - Uses actual database, not in-memory
- **HTTP-based testing** - Tests actual API endpoints

## Common Development Tasks

### Adding New Domain Entities
1. Create domain class inheriting from `Entity` or `AggregateRoot`
2. Add to `StoreDbContext` as `DbSet<T>`
3. Create configuration class in `Data/Configuration/`
4. Generate and apply EF migration
5. Update seeder if needed

### Adding New API Endpoints
1. Add endpoint definition to `Endpoints.StoreApi` in `Pizzeria.Common`
2. Map endpoint in `Program.cs` using minimal APIs pattern:
   ```csharp
   app.MapGet("/pizzas", async (StoreDbContext db, CancellationToken cancellationToken) => { ... });
   ```
3. Inject `StoreDbContext` for data access
4. Follow aggregate root access patterns - always load with `.Include()` for child collections

### Working with Domain Models
```csharp
// Always load aggregate roots with their children
var order = await db.Orders
    .Include(x => x.Pizzas)  // Include child collection
    .FirstOrDefaultAsync(x => x.Id == orderId, cancellationToken);

// Use domain methods rather than direct property manipulation
order.AddPizza(pizza);  // NOT: order.Pizzas.Add(...)

// Let EF track changes through the aggregate root
await db.SaveChangesAsync(cancellationToken);
```

### Debugging Concurrency Issues
1. Enable EF Core sensitive data logging in `appsettings.Development.json`:
   ```json
   {
     "Logging": {
       "LogLevel": {
         "Microsoft.EntityFrameworkCore.Database.Command": "Information"
       }
     }
   }
   ```

2. Use SQL profiler to see actual queries and `xmin` values

3. Check `xmin` values in PostgreSQL directly:
   ```sql
   SELECT id, xmin, created_at_utc FROM sto."Orders" WHERE id = '<order-guid>';
   ```

4. Verify entity tracking states before `SaveChanges()`:
   ```csharp
   foreach (var entry in context.ChangeTracker.Entries())
   {
       Console.WriteLine($"{entry.Entity.GetType().Name}: {entry.State}");
   }
   ```

5. Examine the audit interceptor behavior - it may be causing additional modifications

## Troubleshooting Guide

### Common Issues

**Build Failures**
- Ensure .NET 9 SDK is installed
- Check Docker Desktop is running
- Verify PostgreSQL container can start

**Test Failures** 
- The second test is expected to fail (demonstrates the issue)
- If first test fails, check Docker/PostgreSQL setup
- Use debugger to examine EF change tracking

**Database Connection Issues**
- Verify PostgreSQL container is healthy
- Check connection strings in configuration
- Ensure proper schema (`sto`) exists

### Investigation Approaches
1. **EF Change Tracking** - Examine `context.ChangeTracker.Entries()` to see what EF thinks has changed
2. **SQL Logging** - Enable detailed EF command logging to see actual SQL statements
3. **PostgreSQL Logs** - Check container logs for constraint violations or deadlocks
4. **Version Debugging** - Inspect `xmin` values before/after operations to understand PostgreSQL behavior
5. **Audit Interceptor Impact** - The interceptor updates modification timestamps on aggregate roots even when unchanged - this might affect `xmin`

### Potential Solutions to Investigate
1. **Manual Version Increment** - Explicitly increment `Version` property when child entities change
2. **Custom Concurrency Strategy** - Use a manual timestamp-based approach instead of `xmin`
3. **EF Core Configuration** - Investigate PostgreSQL-specific EF configurations
4. **Interceptor Modification** - Modify the audit interceptor to handle aggregate root versions correctly
5. **Transaction Isolation** - Experiment with different transaction isolation levels

## Configuration Notes

### PostgreSQL-Specific Settings
- Uses `xmin` system column for concurrency via `.IsRowVersion()` in `AggregateRootConfiguration`
- Schema name: `sto` (defined in `StoreDbContext.SchemaName`)
- Connection string managed by Aspire configuration
- Migrations use `NpgsqlMigrationsSqlGenerator` with history table in custom schema

Key differences from SQL Server:
- `xmin` is a PostgreSQL system column that tracks transaction IDs
- Unlike SQL Server's `rowversion`, `xmin` behavior with child entity changes is different
- The concurrency exception occurs because PostgreSQL doesn't update parent `xmin` when child rows change

### Aspire Integration
- Database server: `database-server`
- Database name: `pizza-store-database` 
- API service: `pizza-store-api`
- All service names defined in `ServiceNames` constants

This repository serves as both a reproduction case and a learning example for PostgreSQL EF Core concurrency patterns in .NET applications.