﻿namespace Pizzeria.Store.Api.Domain;

public class OrderPizza
{
    public Guid Id { get; internal set; }

    public Guid OrderId { get; internal set; }

    public Order Order { get; internal set; }

    public Guid PizzaId { get; internal set; }

    public Pizza Pizza { get; internal set; }

    public int Quantity { get; internal set; }

    internal static OrderPizza CreateForOrder(Order order, Pizza pizza)
    {
        return new OrderPizza
        {
            Id = Guid.NewGuid(),
            OrderId = order.Id,
            PizzaId = pizza.Id,
            Quantity = 1
        };
    }

    internal void IncreaseQuantity()
    {
        this.Quantity++;
    }
}
