using MediatR;
using Microsoft.EntityFrameworkCore;
using OrderFlow.Application.Common.Interfaces;
using OrderFlow.Application.Features.Orders.DTOs;

namespace OrderFlow.Application.Features.Orders.Queries.GetOrderById;

/// <summary>
/// Query handler implementing a Read-Through caching pattern via Redis.
/// Architectural Trade-Off:
/// - Cache TTL: Set to 10 minutes. This balances minimizing SQL database load for hot individual order reads
///   while preventing stale data from lingering indefinitely in distributed memory.
/// </summary>
public class GetOrderByIdQueryHandler : IRequestHandler<GetOrderByIdQuery, OrderResponseDto?>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICacheService _cacheService;
    private static readonly TimeSpan CacheTtl = TimeSpan.FromMinutes(10);

    public GetOrderByIdQueryHandler(IApplicationDbContext dbContext, ICacheService cacheService)
    {
        _dbContext = dbContext;
        _cacheService = cacheService;
    }

    public async Task<OrderResponseDto?> Handle(GetOrderByIdQuery request, CancellationToken cancellationToken)
    {
        string cacheKey = $"order:{request.Id}";

        // 1. Check Redis cache first (Hot Read)
        var cachedOrder = await _cacheService.GetAsync<OrderResponseDto>(cacheKey, cancellationToken);
        if (cachedOrder != null)
        {
            return cachedOrder;
        }

        // 2. Cache Miss: Query SQL Server transactional write model
        var order = await _dbContext.Orders
            .AsNoTracking()
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == request.Id, cancellationToken);

        if (order == null)
        {
            return null;
        }

        var orderDto = new OrderResponseDto(
            order.Id,
            order.CustomerName,
            order.Status.ToString(),
            order.CreatedAtUtc,
            order.TotalAmount,
            order.Items.Select(i => new OrderItemDto(
                i.Id,
                i.OrderId,
                i.ProductName,
                i.Quantity,
                i.UnitPrice,
                i.LineTotal
            )).ToList()
        );

        // 3. Populate Redis cache with TTL
        await _cacheService.SetAsync(cacheKey, orderDto, CacheTtl, cancellationToken);

        return orderDto;
    }
}
