namespace OrderFlow.Application.Features.Orders.DTOs;

public record OrderResponseDto(
    Guid Id,
    string CustomerName,
    string Status,
    DateTime CreatedAtUtc,
    decimal TotalAmount,
    List<OrderItemDto> Items);
