namespace Pizzeria.Common;

public static class Endpoints
{
    public static class RouteTokens
    {
        public const string OrderId = "orderId";
        public const string PizzaId = "pizzaId";
    }

    public static class PostgresStoreApi
    {
        public const string Pizzas = "/postgres/pizzas";
        public const string Orders = "/postgres/orders";
        public const string AddPizzaToOrder = $"/postgres/orders/{{{RouteTokens.OrderId}}}/pizzas/{{{RouteTokens.PizzaId}}}";

        public static string GetAddPizzaToOrderEndpoint(Guid orderId, Guid pizzaId)
        {
            return AddPizzaToOrder
                .Replace($"{{{RouteTokens.OrderId}}}", orderId.ToString())
                .Replace($"{{{RouteTokens.PizzaId}}}", pizzaId.ToString());
        }
    }

    public static class SqlServerStoreApi
    {
        public const string Pizzas = "/sqlserver/pizzas";
        public const string Orders = "/sqlserver/orders";
        public const string AddPizzaToOrder = $"/sqlserver/orders/{{{RouteTokens.OrderId}}}/pizzas/{{{RouteTokens.PizzaId}}}";

        public static string GetAddPizzaToOrderEndpoint(Guid orderId, Guid pizzaId)
        {
            return AddPizzaToOrder
                .Replace($"{{{RouteTokens.OrderId}}}", orderId.ToString())
                .Replace($"{{{RouteTokens.PizzaId}}}", pizzaId.ToString());
        }
    }

    // Keep the original for backward compatibility during transition
    public static class StoreApi
    {
        public const string Pizzas = "/pizzas";
        public const string Orders = "/orders";
        public const string AddPizzaToOrder = $"/orders/{{{RouteTokens.OrderId}}}/pizzas/{{{RouteTokens.PizzaId}}}";

        public static string GetAddPizzaToOrderEndpoint(Guid orderId, Guid pizzaId)
        {
            return AddPizzaToOrder
                .Replace($"{{{RouteTokens.OrderId}}}", orderId.ToString())
                .Replace($"{{{RouteTokens.PizzaId}}}", pizzaId.ToString());
        }
    }
}
