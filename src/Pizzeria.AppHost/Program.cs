using Pizzeria.Common;

var builder = DistributedApplication.CreateBuilder(args);

// PostgreSQL server and database
var postgresServer = builder.AddPostgres(ServiceNames.PostgresServer);
var pizzaStorePostgresDatabase = postgresServer.AddDatabase(ServiceNames.PizzaStorePostgresDatabase);

// SQL Server server and database
//var sqlServerServer = builder.AddSqlServer(ServiceNames.SqlServerServer);
//var pizzaStoreSqlServerDatabase = sqlServerServer.AddDatabase(ServiceNames.PizzaStoreSqlServerDatabase);

builder.AddProject<Projects.Pizzeria_Store_Api>(ServiceNames.PizzaStoreApi)
    .WithReference(pizzaStorePostgresDatabase);
    //.WithReference(pizzaStoreSqlServerDatabase);

var app = builder.Build();

await app.RunAsync();
