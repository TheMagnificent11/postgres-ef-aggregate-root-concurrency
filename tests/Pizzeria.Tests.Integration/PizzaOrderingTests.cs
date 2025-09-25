using Pizzeria.Common;
using Xunit;
using Pizzeria.Store.Domain;

namespace Pizzeria.Tests.Integration;

[Collection(PizzeriaApplicationFactory.CollectionName)]
public sealed class PizzaOrderingTests
{
    private readonly PizzeriaApplicationFactory factory;

    public PizzaOrderingTests(PizzeriaApplicationFactory factory)
    {
        this.factory = factory;
    }

    [Fact]
    public async Task Should_CreateOrder_When_OrderIsPlaced_PostgreSQL()
    {
        using var httpClient = await this.factory.GetServiceClientAsync(ServiceNames.PizzaStoreApi);

        await this.CreateOrderAsync(httpClient, Endpoints.PostgresStoreApi.Orders);
    }

    //[Fact]
    //public async Task Should_CreateOrder_When_OrderIsPlaced_SqlServer()
    //{
    //    using var httpClient = await this.factory.GetServiceClientAsync(ServiceNames.PizzaStoreApi);

    //    await this.CreateOrderAsync(httpClient, Endpoints.SqlServerStoreApi.Orders);
    //}

    [Fact]
    public async Task Should_AddPizzaToOrder_When_PizzaIsAdded_PostgreSQL()
    {
        // Arrange
        using var httpClient = await this.factory.GetServiceClientAsync(ServiceNames.PizzaStoreApi);

        var order = await this.CreateOrderAsync(httpClient, Endpoints.PostgresStoreApi.Orders);
        Assert.NotNull(order);
        using var addPizzaRequest = new HttpRequestMessage(
            HttpMethod.Put,
            Endpoints.PostgresStoreApi.GetAddPizzaToOrderEndpoint(order.Id, Menu.PizzaIds.QuattroFormaggi));

        // Act
        using var addPizzaResponse = await httpClient.SendAsync(addPizzaRequest);

        // Assert
        addPizzaResponse.EnsureSuccessStatusCode();

        order = await this.factory.GetPostgresOrder(order.Id);
        Assert.NotNull(order);
        Assert.NotNull(order.Pizzas);
        Assert.Single(order.Pizzas);

        var pizzaInOrder = order.Pizzas.First();
        Assert.Equal(Menu.PizzaIds.QuattroFormaggi, pizzaInOrder.PizzaId);
        Assert.Equal(1, pizzaInOrder.Quantity);
    }

    //[Fact]
    //public async Task Should_AddPizzaToOrder_When_PizzaIsAdded_SqlServer()
    //{
    //    // Arrange
    //    using var httpClient = await this.factory.GetServiceClientAsync(ServiceNames.PizzaStoreApi);

    //    var order = await this.CreateOrderAsync(httpClient, Endpoints.SqlServerStoreApi.Orders);
    //    Assert.NotNull(order);
    //    using var addPizzaRequest = new HttpRequestMessage(
    //        HttpMethod.Put,
    //        Endpoints.SqlServerStoreApi.GetAddPizzaToOrderEndpoint(order.Id, Menu.PizzaIds.QuattroFormaggi));

    //    // Act
    //    using var addPizzaResponse = await httpClient.SendAsync(addPizzaRequest);

    //    // Assert
    //    addPizzaResponse.EnsureSuccessStatusCode();

    //    order = await this.factory.GetSqlServerOrder(order.Id);
    //    Assert.NotNull(order);
    //    Assert.NotNull(order.Pizzas);
    //    Assert.Single(order.Pizzas);

    //    var pizzaInOrder = order.Pizzas.First();
    //    Assert.Equal(Menu.PizzaIds.QuattroFormaggi, pizzaInOrder.PizzaId);
    //    Assert.Equal(1, pizzaInOrder.Quantity);
    //}

    private async Task<Order> CreateOrderAsync(HttpClient httpClient, string endpoint)
    {
        // Arrange
        using var request = new HttpRequestMessage(HttpMethod.Post, endpoint);

        // Act
        using var response = await httpClient.SendAsync(request);

        // Assert
        response.EnsureSuccessStatusCode();

        var order = await this.factory.GetLatestPostgresOrder();
        Assert.NotNull(order);

        return order;
    }
}
