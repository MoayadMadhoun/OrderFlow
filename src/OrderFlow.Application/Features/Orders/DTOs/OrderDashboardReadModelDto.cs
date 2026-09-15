namespace OrderFlow.Application.Features.Orders.DTOs;

public record OrderDashboardReadModelDto(
    Guid Id,
    Guid CustomerId,
    string CustomerName,
    int ItemCount,
    decimal TotalAmount,
    string Status,
    DateTime LastUpdatedUtc);
