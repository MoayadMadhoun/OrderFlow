using MediatR;
using Microsoft.AspNetCore.Mvc;
using OrderFlow.Application.Features.Orders.Commands.CreateOrder;
using OrderFlow.Application.Features.Orders.DTOs;
using OrderFlow.Application.Features.Orders.Queries.GetOrderById;
using OrderFlow.Application.Features.Orders.Queries.ListOrders;

namespace OrderFlow.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly ISender _sender;

    public OrdersController(ISender sender)
    {
        _sender = sender;
    }

    /// <summary>
    /// Creates a new order.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateOrderCommand command)
    {
        var orderId = await _sender.Send(command);
        return CreatedAtAction(nameof(GetById), new { id = orderId }, orderId);
    }

    /// <summary>
    /// Retrieves order details by ID. Utilizes Redis Read-Through Caching.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(OrderResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var order = await _sender.Send(new GetOrderByIdQuery(id));
        if (order == null)
        {
            return NotFound(new { message = $"Order with ID {id} was not found." });
        }

        return Ok(order);
    }

    /// <summary>
    /// Lists all orders with summary metrics.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<OrderSummaryDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> List()
    {
        var orders = await _sender.Send(new ListOrdersQuery());
        return Ok(orders);
    }
}
