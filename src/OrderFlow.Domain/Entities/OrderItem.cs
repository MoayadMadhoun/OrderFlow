using OrderFlow.Domain.Common;
using OrderFlow.Domain.Exceptions;

namespace OrderFlow.Domain.Entities;

public class OrderItem : Entity
{
    public Guid OrderId { get; private set; }
    public string ProductName { get; private set; } = default!;
    public int Quantity { get; private set; }
    public decimal UnitPrice { get; private set; }
    public decimal LineTotal => Quantity * UnitPrice;

    /// <summary>
    /// Parameterless constructor required by Entity Framework Core for materialization.
    /// </summary>
    private OrderItem() { }

    public OrderItem(Guid orderId, string productName, int quantity, decimal unitPrice)
    {
        if (orderId == Guid.Empty)
            throw new DomainException("OrderId cannot be empty.");

        if (string.IsNullOrWhiteSpace(productName))
            throw new DomainException("ProductName is required.");

        if (quantity <= 0)
            throw new DomainException("Quantity must be greater than zero.");

        if (unitPrice < 0)
            throw new DomainException("UnitPrice cannot be negative.");

        OrderId = orderId;
        ProductName = productName;
        Quantity = quantity;
        UnitPrice = unitPrice;
    }
}
