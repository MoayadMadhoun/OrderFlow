namespace OrderFlow.Application.Features.Orders.DTOs;

public record OrderSummaryDto(
    Guid Id,
    string CustomerName,
    string Status,
    DateTime CreatedAtUtc,
    decimal TotalAmount,
    int ItemCount);
