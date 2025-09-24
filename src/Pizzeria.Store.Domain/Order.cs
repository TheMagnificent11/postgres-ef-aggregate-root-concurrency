using System.Diagnostics.CodeAnalysis;

namespace Pizzeria.Store.Domain;

public class Order : AggregateRoot
{
    private readonly List<OrderPizza> pizzas;

    internal Order(string userId)
        : base()
    {
        this.pizzas = [];
        this.UserId = userId;
        this.StartedDateTime = DateTime.UtcNow;
    }

    [ExcludeFromCodeCoverage(Justification = "Only used by EF")]
    private Order()
        : base()
    {
    }

    public string UserId { get; protected set; }

    public IReadOnlyCollection<OrderPizza> Pizzas => this.pizzas;

    public string? DeliveryAddress { get; protected set; }

    public bool IsDeliveryOrder => !string.IsNullOrWhiteSpace(this.DeliveryAddress);

    public DateTime StartedDateTime { get; protected set; }

    public DateTime? SubmittedDateTime { get; protected set; }

    public bool IsSubmitted => this.SubmittedDateTime is not null;

    public DateTime? PreparedDateTime { get; protected set; }

    public bool IsPrepared => this.PreparedDateTime is not null;

    public DateTime? CompletedDateTime { get; protected set; }

    public bool IsCompleted => this.CompletedDateTime is not null;

    public static Order StartNewOrder(string userId)
    {
        return new Order(userId);
    }

    public void AddPizza(Pizza pizza)
    {
        var existingOrderPizza = this.pizzas.FirstOrDefault(x => x.PizzaId == pizza.Id);

        if (existingOrderPizza is null)
        {
            this.pizzas.Add(OrderPizza.CreateForOrder(this, pizza));
            return;
        }

        existingOrderPizza.IncreaseQuantity();
    }

    public static class FieldLengths
    {
        public const int UserId = 100;
        public const int DeliveryAddress = 200;
    }
}