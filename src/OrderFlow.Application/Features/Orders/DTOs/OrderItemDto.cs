namespace OrderFlow.Application.Features.Orders.DTOs;

public record OrderItemDto(
    Guid Id,
    Guid OrderId,
    string ProductName,
    int Quantity,
    decimal UnitPrice,
    decimal LineTotal);
