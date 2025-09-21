using Pizzeria.Common;
using Xunit;
using Pizzeria.Store.Api.Domain;

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
    public async Task Should_CreateOrder_When_OrderIsPlaced()
    {
        // Arrange
        using var httpClient = await this.factory.GetServiceClientAsync(ServiceNames.PizzaStoreApi);
        using var request = new HttpRequestMessage(HttpMethod.Post, Endpoints.StoreApi.Orders);

        // Act
        using var response = await httpClient.SendAsync(request);

        // Assert
        response.EnsureSuccessStatusCode();

        var order = await this.factory.GetLatestOrder();
        order.Should().NotBeNull();
    }

    [Fact]
    public async Task Should_AddPizzaToOrder_When_PizzaIsAdded()
    {
        // Arrange
        using var httpClient = await this.factory.GetServiceClientAsync(ServiceNames.PizzaStoreApi);
        using var createOrderRequest = new HttpRequestMessage(HttpMethod.Post, Endpoints.StoreApi.Orders);
        using var createOrderResponse = await httpClient.SendAsync(createOrderRequest);

        createOrderResponse.EnsureSuccessStatusCode();
        var order = await this.factory.GetLatestOrder();
        order.Should().NotBeNull();
        using var addPizzaRequest = new HttpRequestMessage(
            HttpMethod.Put,
            Endpoints.StoreApi.GetAddPizzaToOrderEndpoint(order.Id, Menu.PizzaIds.QuattroFormaggi));

        // Act
        using var addPizzaResponse = await httpClient.SendAsync(addPizzaRequest);

        // Assert
        addPizzaResponse.EnsureSuccessStatusCode();

        order = await this.factory.GetOrder(order.Id);
        Assert.NotNull(order);
        Assert.NotNull(order.Pizzas);
        Assert.Single(order.Pizzas);

        var pizzaInOrder = order.Pizzas.First();
        Assert.Equal(Menu.PizzaIds.QuattroFormaggi, pizzaInOrder.PizzaId);
        Assert.Equal(1, pizzaInOrder.Quantity);
    }
}
