using MediatR;
using Microsoft.EntityFrameworkCore;
using OrderFlow.Application.Common.Interfaces;
using OrderFlow.Application.Features.Orders.DTOs;

namespace OrderFlow.Application.Features.Orders.Queries.GetOrderDashboard;

/// <summary>
/// Query handler reading exclusively from the SQL Server Materialized View table (OrderDashboardReadModel).
/// Architectural Trade-Off:
/// - Fast Read Performance: Serves pre-calculated summary metrics without calculating aggregates or executing JOINs across transactional tables.
/// - Isolation: Complete separation between Write side (Commands) and Read side (Dashboard Query).
/// </summary>
public class GetOrderDashboardQueryHandler : IRequestHandler<GetOrderDashboardQuery, List<OrderDashboardReadModelDto>>
{
    private readonly IApplicationDbContext _dbContext;

    public GetOrderDashboardQueryHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<OrderDashboardReadModelDto>> Handle(GetOrderDashboardQuery request, CancellationToken cancellationToken)
    {
        return await _dbContext.OrderDashboardReadModels
            .AsNoTracking()
            .OrderByDescending(d => d.LastUpdatedUtc)
            .Select(d => new OrderDashboardReadModelDto(
                d.Id,
                d.CustomerId,
                d.CustomerName,
                d.ItemCount,
                d.TotalAmount,
                d.Status,
                d.LastUpdatedUtc
            ))
            .ToListAsync(cancellationToken);
    }
}
