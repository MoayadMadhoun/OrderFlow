using MediatR;
using OrderFlow.Application.Common.Interfaces;
using OrderFlow.Domain.Entities;

namespace OrderFlow.Application.Features.Orders.Commands.CreateOrder;

public class CreateOrderCommandHandler : IRequestHandler<CreateOrderCommand, Guid>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICacheService _cacheService;

    public CreateOrderCommandHandler(IApplicationDbContext dbContext, ICacheService cacheService)
    {
        _dbContext = dbContext;
        _cacheService = cacheService;
    }

    public async Task<Guid> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        var order = new Order(request.CustomerName);

        foreach (var item in request.Items)
        {
            order.AddItem(item.ProductName, item.Quantity, item.UnitPrice);
        }

        _dbContext.Orders.Add(order);
        await _dbContext.SaveChangesAsync(cancellationToken);

        // Invalidate cached order list to ensure stale reads do not persist
        await _cacheService.RemoveAsync("orders-list", cancellationToken);

        return order.Id;
    }
}
