using MediatR;
using OrderFlow.Application.Features.Orders.DTOs;

namespace OrderFlow.Application.Features.Orders.Commands.CreateOrder;

public record CreateOrderCommand(
    string CustomerName,
    List<CreateOrderItemCommandDto> Items) : IRequest<Guid>;
