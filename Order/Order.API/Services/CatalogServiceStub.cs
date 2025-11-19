using Order.Application.DTOs;
using Order.Application.Features.Orders;
using Order.Application.Interfaces;

namespace Order.API.Services;

public class CatalogServiceStub : ICatalogService
{
    public Task<ProductAvailabilityResult> CheckProductAvailabilityAsync(List<OrderItemDto> items)
    {
        return Task.FromResult(new ProductAvailabilityResult { IsAvailable = true });
    }

    public Task<ProductAvailabilityResult> CheckProductAvailabilityAsync(List<CreateOrderItem> orderItems)
    {
        return Task.FromResult(new ProductAvailabilityResult { IsAvailable = true });
    }

    public Task ReserveProductsAsync(List<OrderItemDto> items)
    {
        return Task.CompletedTask;
    }

    public Task ReleaseProductsAsync(List<OrderItemDto> items)
    {
        return Task.CompletedTask;
    }

    public Task ReserveProductsAsync(List<CreateOrderItem> orderItems)
    {
        return Task.CompletedTask;
    }
}


