using MediatR;
using OrderFlow.Application.Features.Orders.DTOs;

namespace OrderFlow.Application.Features.Orders.Queries.ListOrders;

public record ListOrdersQuery() : IRequest<List<OrderSummaryDto>>;
