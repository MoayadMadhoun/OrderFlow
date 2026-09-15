using MediatR;
using OrderFlow.Application.Features.Orders.DTOs;

namespace OrderFlow.Application.Features.Orders.Queries.GetOrderDashboard;

public record GetOrderDashboardQuery() : IRequest<List<OrderDashboardReadModelDto>>;
