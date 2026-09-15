using OrderFlow.Domain.Common;
using OrderFlow.Domain.Enums;
using OrderFlow.Domain.Exceptions;

namespace OrderFlow.Domain.Entities;

public class Order : Entity
{
    public string CustomerName { get; private set; } = default!;
    public OrderStatus Status { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }

    private readonly List<OrderItem> _items = new();
    public IReadOnlyCollection<OrderItem> Items => _items.AsReadOnly();

    public decimal TotalAmount => _items.Sum(item => item.LineTotal);

    /// <summary>
    /// Parameterless constructor required by Entity Framework Core.
    /// </summary>
    private Order() { }

    public Order(string customerName)
    {
        if (string.IsNullOrWhiteSpace(customerName))
            throw new DomainException("Customer name is required.");

        CustomerName = customerName;
        Status = OrderStatus.Pending;
        CreatedAtUtc = DateTime.UtcNow;
    }

    /// <summary>
    /// Encapsulates adding items to the order while preserving domain invariants.
    /// </summary>
    public void AddItem(string productName, int quantity, decimal unitPrice)
    {
        if (Status != OrderStatus.Pending)
            throw new DomainException("Items can only be added to pending orders.");

        var item = new OrderItem(Id, productName, quantity, unitPrice);
        _items.Add(item);
    }

    /// <summary>
    /// Transitions order state from Pending to Completed.
    /// </summary>
    public void CompleteOrder()
    {
        if (Status != OrderStatus.Pending)
            throw new DomainException($"Only pending orders can be completed. Current status: {Status}");

        Status = OrderStatus.Completed;
    }

    /// <summary>
    /// Transitions order state from Pending to Cancelled.
    /// </summary>
    public void CancelOrder()
    {
        if (Status != OrderStatus.Pending)
            throw new DomainException($"Only pending orders can be cancelled. Current status: {Status}");

        Status = OrderStatus.Cancelled;
    }
}