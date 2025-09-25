# postgres-ef-aggregate-root-concurrency

## Purpose

I've always used SQL Sever with Entity Framework, but I recently acquired an ARM64 laptop and SQL Server doesn't run natively on ARM64.

So I decided to try PostgreSQL with Entity Framework Core.

This repository initially was created to investigate differences between PostgreSQL and SQL Server EF Core behavior with concurrency tokens in Domain-Driven Design scenarios.

However, after adding SQL Server implementation (PR #10), it was discovered that both PostgreSQL and SQL Server exhibit the same behavior: when adding child entities to an aggregate root, the EF change tracker appears to treat the child entity addition as a modification rather than an addition.

This is repository with a more minimal (I understand that this is not exactly minimal, but I wanted to keep the code as close as possible to my real project) example that reproduces the issue with both database providers.

## Issue

I want to use EF in a DDD way, so I have aggregate roots and entities.

I want all reads and writes to go via aggregate roots.

So, I have a concurrency token on the aggregate root, and I want to make sure that when I update an entity inside the aggregate root.

**Key Finding**: After implementing both PostgreSQL and SQL Server versions, it appears that **both database providers exhibit the same behavior**. When you read an aggregate root from the `DbContext` and then add a child entity to the aggregate root, the EF change tracker appears to think the child entity is a modification, not an addition.

### PostgreSQL Behavior

I have encountered the following exception when trying to add a record to a child collection of an aggregate root using PostgreSQL:

```bash
Microsoft.EntityFrameworkCore.DbUpdateConcurrencyException
  HResult=0x80131500
  Message=The database operation was expected to affect 1 row(s), but actually affected 0 row(s); data may have been modified or deleted since entities were loaded. See https://go.microsoft.com/fwlink/?LinkId=527962 for information on understanding and handling optimistic concurrency exceptions.
  Source=Npgsql.EntityFrameworkCore.PostgreSQL
  StackTrace:
   at Npgsql.EntityFrameworkCore.PostgreSQL.Update.Internal.NpgsqlModificationCommandBatch.<ThrowAggregateUpdateConcurrencyExceptionAsync>d__10.MoveNext()
   at Npgsql.EntityFrameworkCore.PostgreSQL.Update.Internal.NpgsqlModificationCommandBatch.<Consume>d__7.MoveNext()
   at Microsoft.EntityFrameworkCore.Update.ReaderModificationCommandBatch.<ExecuteAsync>d__50.MoveNext()
   at Microsoft.EntityFrameworkCore.Update.ReaderModificationCommandBatch.<ExecuteAsync>d__50.MoveNext()
   at Microsoft.EntityFrameworkCore.Update.Internal.BatchExecutor.<ExecuteAsync>d__9.MoveNext()
   at Microsoft.EntityFrameworkCore.Update.Internal.BatchExecutor.<ExecuteAsync>d__9.MoveNext()
   at Microsoft.EntityFrameworkCore.Update.Internal.BatchExecutor.<ExecuteAsync>d__9.MoveNext()
   at Microsoft.EntityFrameworkCore.Storage.RelationalDatabase.<SaveChangesAsync>d__8.MoveNext()
   at Microsoft.EntityFrameworkCore.ChangeTracking.Internal.StateManager.<SaveChangesAsync>d__111.MoveNext()
   at Microsoft.EntityFrameworkCore.ChangeTracking.Internal.StateManager.<SaveChangesAsync>d__115.MoveNext()
   at Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal.NpgsqlExecutionStrategy.<ExecuteAsync>d__7`2.MoveNext()
   at Microsoft.EntityFrameworkCore.DbContext.<SaveChangesAsync>d__63.MoveNext()
   at Pizzeria.Store.Api.Handlers.AddPizzaToOrderHandler.<HandleAsync>d__0.MoveNext() in C:\Users\sajiw\source\repos\postgres-ef-aggregate-root-concurrency\src\Pizzeria.Store.Api\Handlers\AddPizzaToOrderHandler.cs:line 34
```

This works with SQL Server EF Core; it may not evaluate the concurrency token on the aggregate root, but it does not encounter any exception.

**Update**: After implementing SQL Server alongside PostgreSQL (PR #10), it was discovered that SQL Server exhibits the same underlying issue. Both database providers show that the EF change tracker treats child entity additions as modifications when working with aggregate roots.

## User Case

The applicatio is the start of simple Pizzeria.

`Pizzeria.Store.Api` has endpoints for both PostgreSQL and SQL Server:

**PostgreSQL endpoints:**
- `GET /postgres/pizzas` to get the list of available pizzas on the menu
- `POST /postgres/orders` to create a new order
- `PUT /postgres/orders/{orderId}/pizzas/{pizzaId}` to add a pizza to an existing order

**SQL Server endpoints:**
- `GET /sqlserver/pizzas` to get the list of available pizzas on the menu
- `POST /sqlserver/orders` to create a new order
- `PUT /sqlserver/orders/{orderId}/pizzas/{pizzaId}` to add a pizza to an existing order

## Aspire

This repository using .Net Aspire to make configuration easier.

Also, the Aspire testing library makes it very easy to write integration tests.

## Prerequisites

- .Net 9
- Docker Desktop

## How to reproduce the issue

1. Clone the repository
2. Navigate to the repository folder
3. Execute `dotnet test --logger console --verbosity:detailed` in the terminal

The tests demonstrate the behavior across both database providers:

**Tests that create orders (should pass):**
- `Should_CreateOrder_When_OrderIsPlaced_PostgreSQL` 
- `Should_CreateOrder_When_OrderIsPlaced_SqlServer`

**Tests that add pizzas to existing orders (demonstrate EF change tracking behavior):**
- `Should_AddPizzaToOrder_When_PizzaIsAdded_PostgreSQL`
- `Should_AddPizzaToOrder_When_PizzaIsAdded_SqlServer`

Both "add pizza" tests reveal that the EF change tracker treats child entity additions as modifications when working through aggregate roots, regardless of the database provider used.
