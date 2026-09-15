using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OrderFlow.Application.Common.Interfaces;
using OrderFlow.Application.Common.Models;
using OrderFlow.Domain.Enums;

namespace OrderFlow.Infrastracture.BackgroundServices;

/// <summary>
/// BackgroundService for asynchronous worker tasks:
/// 1. Periodically transitions 'Pending' orders to 'Completed' (simulating order fulfillment engine).
/// 2. Asynchronously refreshes and upserts the SQL Server Materialized View table (OrderDashboardReadModel) from transactional tables.
/// </summary>
public class OrderBackgroundProcessor : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<OrderBackgroundProcessor> _logger;
    private static readonly TimeSpan ProcessInterval = TimeSpan.FromSeconds(15);

    public OrderBackgroundProcessor(
        IServiceScopeFactory scopeFactory,
        ILogger<OrderBackgroundProcessor> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("OrderBackgroundProcessor started running.");

        using var timer = new PeriodicTimer(ProcessInterval);

        while (!stoppingToken.IsCancellationRequested && await timer.WaitForNextTickAsync(stoppingToken))
        {
            try
            {
                await ProcessPendingOrdersAsync(stoppingToken);
                await RefreshDashboardReadModelAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred during background worker processing execution.");
            }
        }

        _logger.LogInformation("OrderBackgroundProcessor background task is stopping.");
    }

    /// <summary>
    /// Processes pending order transitions (Pending -> Completed).
    /// Creates a dedicated scope per execution cycle to prevent memory leaks and track EF Core entities cleanly.
    /// </summary>
    private async Task ProcessPendingOrdersAsync(CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();
        var cacheService = scope.ServiceProvider.GetRequiredService<ICacheService>();

        var pendingOrders = await dbContext.Orders
            .Include(o => o.Items)
            .Where(o => o.Status == OrderStatus.Pending)
            .ToListAsync(cancellationToken);

        if (pendingOrders.Count == 0)
        {
            return;
        }

        _logger.LogInformation("Processing {Count} pending orders for completion.", pendingOrders.Count);

        foreach (var order in pendingOrders)
        {
            order.CompleteOrder();

            // Evict order details from cache so subsequent GET /api/orders/{id} calls retrieve updated status
            await cacheService.RemoveAsync($"order:{order.Id}", cancellationToken);
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        // Evict list cache
        await cacheService.RemoveAsync("orders-list", cancellationToken);

        _logger.LogInformation("Successfully transitioned {Count} orders to Completed.", pendingOrders.Count);
    }

    /// <summary>
    /// Aggregates transactional data and updates/upserts the SQL Server Materialized View table.
    /// </summary>
    private async Task RefreshDashboardReadModelAsync(CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();

        // Aggregate orders per customer from transactional write model
        var customerAggregates = await dbContext.Orders
            .AsNoTracking()
            .Include(o => o.Items)
            .GroupBy(o => o.CustomerName)
            .Select(g => new
            {
                CustomerName = g.Key,
                ItemCount = g.Sum(o => o.Items.Sum(i => i.Quantity)),
                TotalAmount = g.Sum(o => o.Items.Sum(i => i.Quantity * i.UnitPrice)),
                LatestStatus = g.OrderByDescending(o => o.CreatedAtUtc).Select(o => o.Status.ToString()).FirstOrDefault() ?? "N/A"
            })
            .ToListAsync(cancellationToken);

        var existingReadModels = await dbContext.OrderDashboardReadModels.ToListAsync(cancellationToken);

        foreach (var agg in customerAggregates)
        {
            var existing = existingReadModels.FirstOrDefault(r => r.CustomerName == agg.CustomerName);
            if (existing != null)
            {
                existing.ItemCount = agg.ItemCount;
                existing.TotalAmount = agg.TotalAmount;
                existing.Status = agg.LatestStatus;
                existing.LastUpdatedUtc = DateTime.UtcNow;
            }
            else
            {
                dbContext.OrderDashboardReadModels.Add(new OrderDashboardReadModel
                {
                    Id = Guid.NewGuid(),
                    CustomerId = Guid.NewGuid(),
                    CustomerName = agg.CustomerName,
                    ItemCount = agg.ItemCount,
                    TotalAmount = agg.TotalAmount,
                    Status = agg.LatestStatus,
                    LastUpdatedUtc = DateTime.UtcNow
                });
            }
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Refreshed OrderDashboardReadModel table with {Count} aggregate records.", customerAggregates.Count);
    }
}
