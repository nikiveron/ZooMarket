using Order.Application.DTOs;
using Order.Application.Features.Orders;

namespace Order.Application.Interfaces;

public interface ICatalogService
{
    Task<ProductAvailabilityResult> CheckProductAvailabilityAsync(List<OrderItemDto> items);
    Task<ProductAvailabilityResult> CheckProductAvailabilityAsync(List<CreateOrderItem> orderItems);
    Task ReserveProductsAsync(List<OrderItemDto> items);
    Task ReleaseProductsAsync(List<OrderItemDto> items);
    Task ReserveProductsAsync(List<CreateOrderItem> orderItems);
}

public class ProductAvailabilityResult
{
    public bool IsAvailable { get; set; }
    public List<UnavailableProduct> UnavailableProducts { get; set; } = new();
}

public class UnavailableProduct
{
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int RequestedQuantity { get; set; }
    public int AvailableQuantity { get; set; }
}