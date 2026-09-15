namespace OrderFlow.Application.Common.Models;

/// <summary>
/// SQL Server Read Model (Materialized View table) for high-performance dashboard queries.
/// Architectural Trade-Off:
/// - Write Model (Order & OrderItem): Transactionally consistent, normalized tables optimized for commands & state changes.
/// - Read Model (OrderDashboardReadModel): Pre-aggregated denormalized table updated asynchronously via BackgroundService.
///   Trading eventual consistency for lightning-fast read operations without complex JOINs or aggregations on hot transactional tables.
/// </summary>
public class OrderDashboardReadModel
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid CustomerId { get; set; } = Guid.NewGuid();
    public string CustomerName { get; set; } = default!;
    public int ItemCount { get; set; }
    public decimal TotalAmount { get; set; }
    public string Status { get; set; } = default!;
    public DateTime LastUpdatedUtc { get; set; }
}
