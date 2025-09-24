namespace Pizzeria.Common;

public static class ServiceNames
{
    public const string PostgresServer = "postgres-server";
    public const string SqlServerServer = "sqlserver-server";

    public const string PizzaStorePostgresDatabase = "pizza-store-postgres-database";
    public const string PizzaStoreSqlServerDatabase = "pizza-store-sqlserver-database";

    public const string PizzaStoreApi = "pizza-store-api";

    // Keep the old name for backward compatibility during transition
    public const string DatabaseServer = "database-server";
    public const string PizzaStoreDatabase = "pizza-store-database";
}
