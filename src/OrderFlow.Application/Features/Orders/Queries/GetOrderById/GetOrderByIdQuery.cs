using MediatR;
using OrderFlow.Application.Features.Orders.DTOs;

namespace OrderFlow.Application.Features.Orders.Queries.GetOrderById;

public record GetOrderByIdQuery(Guid Id) : IRequest<OrderResponseDto?>;
