using Microsoft.EntityFrameworkCore;
using OrderFlow.Application.Common.Models;
using OrderFlow.Domain.Entities;

namespace OrderFlow.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<Order> Orders { get; }
    DbSet<OrderItem> OrderItems { get; }
    DbSet<OrderDashboardReadModel> OrderDashboardReadModels { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
