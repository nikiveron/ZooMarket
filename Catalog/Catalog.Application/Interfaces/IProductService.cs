using Catalog.Application.DTOs;
using Catalog.Application.Features.Products;

namespace Catalog.Application.Interfaces;

public interface IProductService
{
    Task<ProductDto> GetProductByIdAsync(Guid id);
    Task<IEnumerable<ProductDto>> GetAllProductsAsync();
    Task<IEnumerable<ProductDto>> GetProductsByCategoryAsync(Guid categoryId);
    Task<ProductDto> CreateProductAsync(CreateProductCommand command);
    Task UpdateProductAsync(UpdateProductCommand command);
    Task DeleteProductAsync(Guid id);
    Task UpdateStockAsync(Guid productId, int newStockQuantity);
}