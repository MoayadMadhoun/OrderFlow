namespace OrderFlow.Application.Features.Orders.DTOs;

public record CreateOrderItemCommandDto(
    string ProductName,
    int Quantity,
    decimal UnitPrice);
