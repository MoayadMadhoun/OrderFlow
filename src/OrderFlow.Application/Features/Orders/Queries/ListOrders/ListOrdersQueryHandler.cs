using MediatR;
using Microsoft.EntityFrameworkCore;
using OrderFlow.Application.Common.Interfaces;
using OrderFlow.Application.Features.Orders.DTOs;

namespace OrderFlow.Application.Features.Orders.Queries.ListOrders;

public class ListOrdersQueryHandler : IRequestHandler<ListOrdersQuery, List<OrderSummaryDto>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICacheService _cacheService;
    private static readonly string CacheKey = "orders-list";

    public ListOrdersQueryHandler(IApplicationDbContext dbContext, ICacheService cacheService)
    {
        _dbContext = dbContext;
        _cacheService = cacheService;
    }

    public async Task<List<OrderSummaryDto>> Handle(ListOrdersQuery request, CancellationToken cancellationToken)
    {
        var cachedList = await _cacheService.GetAsync<List<OrderSummaryDto>>(CacheKey, cancellationToken);
        if (cachedList != null)
        {
            return cachedList;
        }

        var orders = await _dbContext.Orders
            .AsNoTracking()
            .OrderByDescending(o => o.CreatedAtUtc)
            .Select(o => new OrderSummaryDto(
                o.Id,
                o.CustomerName,
                o.Status.ToString(),
                o.CreatedAtUtc,
                o.TotalAmount,
                o.Items.Count
            ))
            .ToListAsync(cancellationToken);

        await _cacheService.SetAsync(CacheKey, orders, TimeSpan.FromMinutes(2), cancellationToken);

        return orders;
    }
}
