# Copilot Instructions for PostgreSQL EF Aggregate Root Concurrency

## Repository Overview

This repository demonstrates Entity Framework Core behavior when working with Domain-Driven Design aggregate roots and child entity modifications. It's a minimal reproduction case for a pizzeria application that uses .NET Aspire for orchestration and tests both PostgreSQL and SQL Server providers.

### Purpose
- Demonstrates EF Core behavior with aggregate roots across both PostgreSQL and SQL Server
- Shows how EF change tracker treats child entity additions as modifications when working through aggregate roots
- Provides a test case for investigating and understanding this EF Core behavior pattern

## Architecture & Technology Stack

### Core Technologies
- **.NET 9** - Latest .NET framework
- **.NET Aspire** - Application orchestration and configuration
- **Entity Framework Core** - ORM with both PostgreSQL and SQL Server providers
- **PostgreSQL** - Database with `xmin` concurrency tokens
- **SQL Server** - Database with `rowversion` concurrency tokens  
- **xUnit** - Testing framework with Aspire integration testing
- **Docker** - Database containerization

### Project Structure
```
src/
├── Pizzeria.AppHost/           # .NET Aspire orchestrator
├── Pizzeria.ServiceDefaults/   # Shared Aspire service configuration  
├── Pizzeria.Common/           # Shared constants and utilities
├── Pizzeria.Store.Api/        # Main API with domain models and data layer
│   ├── Postgres/              # PostgreSQL-specific DbContext and migrations
│   └── SqlServer/             # SQL Server-specific DbContext and migrations
├── Pizzeria.Store.Application/ # Application handlers
├── Pizzeria.Store.Data/       # Shared data layer components
└── Pizzeria.Store.Domain/     # Domain entities and aggregate roots
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
- **Concurrency Control** - Uses `uint Version` property mapped to database-specific concurrency tokens (`xmin` for PostgreSQL, `rowversion` for SQL Server)
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
The API uses minimal APIs with separate endpoints for each database provider:

**PostgreSQL endpoints:**
- `GET /postgres/pizzas` - Retrieve menu items
- `POST /postgres/orders` - Create new order (works correctly)  
- `PUT /postgres/orders/{orderId}/pizzas/{pizzaId}` - Add pizza to order (demonstrates EF change tracking behavior)

**SQL Server endpoints:**
- `GET /sqlserver/pizzas` - Retrieve menu items
- `POST /sqlserver/orders` - Create new order (works correctly)
- `PUT /sqlserver/orders/{orderId}/pizzas/{pizzaId}` - Add pizza to order (demonstrates EF change tracking behavior)

Route constants are defined in `Pizzeria.Common.Endpoints` for consistency across projects.

## The EF Core Change Tracking Behavior

### Problem Description
When adding a child entity to an existing aggregate root, both PostgreSQL and SQL Server EF Core exhibit the same behavior: the EF change tracker appears to treat the child entity addition as a modification rather than an addition.

This can lead to:
- **PostgreSQL**: `DbUpdateConcurrencyException` due to `xmin` version conflicts
- **SQL Server**: Similar underlying change tracking behavior, though may not always result in exceptions

### Root Cause Analysis
- Both PostgreSQL (`xmin`) and SQL Server (`rowversion`) use database-specific concurrency tokens
- EF Core maps `AggregateRoot.Version` to these tokens via `.IsRowVersion()`
- The `AuditDetailsSaveChangesInterceptor` modifies aggregate roots even when marked as `Unchanged`
- Child entity modifications don't properly coordinate with parent aggregate version handling
- This leads to inconsistent change tracking state during `SaveChanges()`

### Test Reproduction
```csharp
// These tests pass - creating orders
[Fact] Should_CreateOrder_When_OrderIsPlaced_PostgreSQL()
[Fact] Should_CreateOrder_When_OrderIsPlaced_SqlServer()

// These tests demonstrate EF change tracking behavior with child entities
[Fact] Should_AddPizzaToOrder_When_PizzaIsAdded_PostgreSQL()
[Fact] Should_AddPizzaToOrder_When_PizzaIsAdded_SqlServer()
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
// AppHost setup for both database providers
var postgresServer = builder.AddPostgres(ServiceNames.DatabaseServer);
var pizzaStorePostgresDb = postgresServer.AddDatabase(ServiceNames.PizzaStorePostgresDatabase);

var sqlServerDb = builder.AddSqlServer(ServiceNames.SqlServerDatabase)
    .PublishAsConnectionString()
    .AddDatabase(ServiceNames.PizzaStoreSqlServerDatabase);

// Service registration with both database dependencies
builder.AddProject<Projects.Pizzeria_Store_Api>(ServiceNames.PizzaStoreApi)
    .WithReference(pizzaStorePostgresDb)
    .WithReference(sqlServerDb)
    .WaitFor(pizzaStorePostgresDb)
    .WaitFor(sqlServerDb);
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

**Important Note**: The interceptor's behavior of updating aggregate roots marked as `Unchanged` when child entities are added contributes to the change tracking behavior observed in both PostgreSQL and SQL Server implementations.

## EF Core Change Tracking with Aggregate Roots

### The Core Behavior Pattern

When working with aggregate roots and child entities, both PostgreSQL and SQL Server EF Core implementations exhibit a consistent pattern:

1. **Load aggregate root with child collection** via `.Include()`
2. **Add child entity** through aggregate root domain method  
3. **EF change tracker** treats the operation as a modification rather than a pure addition
4. **Audit interceptor** modifies the aggregate root even when marked as `Unchanged`
5. **SaveChanges()** encounters conflicts due to concurrency token handling

### Change Tracking State Analysis

```csharp
// Before SaveChanges(), you might observe:
// Order (aggregate root): EntityState.Unchanged -> Modified (due to audit interceptor)
// OrderPizza (child): EntityState.Added
// This combination can cause concurrency conflicts
```

### Cross-Provider Consistency

The key insight is that this behavior is **consistent across database providers**:
- PostgreSQL with `xmin` concurrency tokens
- SQL Server with `rowversion` concurrency tokens  
- Both exhibit the same underlying EF change tracking pattern
- The manifestation may differ (exceptions vs. warnings) but the root cause is identical

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
// Always load aggregate roots with their children (both PostgreSQL and SQL Server)
var order = await db.Orders
    .Include(x => x.Pizzas)  // Include child collection
    .FirstOrDefaultAsync(x => x.Id == orderId, cancellationToken);

// Use domain methods rather than direct property manipulation
order.AddPizza(pizza);  // NOT: order.Pizzas.Add(...)

// Let EF track changes through the aggregate root
await db.SaveChangesAsync(cancellationToken);
```

### Debugging EF Change Tracking Issues
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

2. Use SQL profiler to see actual queries and concurrency token values

3. Check concurrency token values directly in databases:
   ```sql
   -- PostgreSQL
   SELECT id, xmin, created_at_utc FROM sto."Orders" WHERE id = '<order-guid>';
   
   -- SQL Server  
   SELECT id, version, created_at_utc FROM sto.Orders WHERE id = '<order-guid>';
   ```

4. Verify entity tracking states before `SaveChanges()`:
   ```csharp
   foreach (var entry in context.ChangeTracker.Entries())
   {
       Console.WriteLine($"{entry.Entity.GetType().Name}: {entry.State}");
   }
   ```

5. Examine the audit interceptor behavior - it modifies aggregate roots even when marked as `Unchanged`

## Troubleshooting Guide

### Common Issues

**Build Failures**
- Ensure .NET 9 SDK is installed
- Check Docker Desktop is running
- Verify PostgreSQL container can start

**Test Failures** 
- Tests may reveal EF change tracking behavior patterns with aggregate roots
- If infrastructure tests fail, check Docker/database setup
- Use debugger to examine EF change tracking states

**Database Connection Issues**
- Verify PostgreSQL and SQL Server containers are healthy
- Check connection strings in configuration
- Ensure proper schemas exist (`sto` for both providers)

### Investigation Approaches
1. **EF Change Tracking** - Examine `context.ChangeTracker.Entries()` to see what EF thinks has changed
2. **SQL Logging** - Enable detailed EF command logging to see actual SQL statements for both providers
3. **Database Logs** - Check container logs for constraint violations or deadlocks
4. **Version Debugging** - Inspect concurrency token values before/after operations to understand database behavior
5. **Audit Interceptor Impact** - The interceptor updates modification timestamps on aggregate roots even when unchanged - this affects change tracking
6. **Cross-Provider Comparison** - Compare behavior between PostgreSQL and SQL Server to identify common patterns

### Potential Solutions to Investigate
1. **Manual Version Increment** - Explicitly increment `Version` property when child entities change
2. **Custom Concurrency Strategy** - Use a manual timestamp-based approach instead of database-specific tokens
3. **EF Core Configuration** - Investigate provider-specific EF configurations
4. **Interceptor Modification** - Modify the audit interceptor to handle aggregate root versions correctly across both providers
5. **Transaction Isolation** - Experiment with different transaction isolation levels
6. **Change Tracking Optimization** - Refine how EF tracks changes to child entities within aggregate boundaries

## Configuration Notes

### Database-Specific Settings

**PostgreSQL-Specific:**
- Uses `xmin` system column for concurrency via `.IsRowVersion()` in `AggregateRootConfiguration`
- Schema name: `sto` (defined in `StorePostgresDbContext.SchemaName`)
- Migrations use `NpgsqlMigrationsSqlGenerator` with history table in custom schema

**SQL Server-Specific:**
- Uses `rowversion` column for concurrency via `.IsRowVersion()` in `AggregateRootConfiguration`  
- Schema name: `sto` (defined in `StoreSqlServerDbContext.SchemaName`)
- Migrations use standard SQL Server migration generator

Key behavioral differences:
- `xmin` is a PostgreSQL system column that tracks transaction IDs
- `rowversion` is a SQL Server binary column that increments automatically
- Both exhibit similar EF change tracking behavior with aggregate roots and child entities
- The concurrency issue manifests differently but stems from the same EF change tracking pattern

### Aspire Integration
- PostgreSQL server: `database-server`
- PostgreSQL database: `pizza-store-postgres-database`
- SQL Server database: `pizza-store-sqlserver-database`
- API service: `pizza-store-api`
- All service names defined in `ServiceNames` constants

This repository serves as both a reproduction case and a learning example for understanding EF Core change tracking behavior with aggregate roots across different database providers in .NET applications.