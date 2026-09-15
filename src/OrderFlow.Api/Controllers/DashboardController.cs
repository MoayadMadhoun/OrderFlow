using MediatR;
using Microsoft.AspNetCore.Mvc;
using OrderFlow.Application.Features.Orders.DTOs;
using OrderFlow.Application.Features.Orders.Queries.GetOrderDashboard;

namespace OrderFlow.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DashboardController : ControllerBase
{
    private readonly ISender _sender;

    public DashboardController(ISender sender)
    {
        _sender = sender;
    }

    /// <summary>
    /// Retrieves pre-aggregated order dashboard metrics exclusively from SQL Server Materialized View table.
    /// </summary>
    [HttpGet("orders")]
    [ProducesResponseType(typeof(List<OrderDashboardReadModelDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetOrderDashboard()
    {
        var dashboardData = await _sender.Send(new GetOrderDashboardQuery());
        return Ok(dashboardData);
    }
}
