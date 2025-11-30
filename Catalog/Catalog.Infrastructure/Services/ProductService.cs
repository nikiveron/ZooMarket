using Catalog.Application.DTOs;
using Catalog.Application.Features.Products;
using Catalog.Application.Interfaces;
using Catalog.Domain.Common;
using Catalog.Domain.Entities;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

namespace Catalog.Infrastructure.Services;

public class ProductService : IProductService
{
    private readonly IRepository<ProductEntity> _productRepository;
    private readonly IRepository<Category> _categoryRepository;
    private readonly IDistributedCache _cache;
    private const string CachePrefix = "product_";

    public ProductService(
        IRepository<ProductEntity> productRepository,
        IRepository<Category> categoryRepository,
        IDistributedCache cache)
    {
        _productRepository = productRepository;
        _categoryRepository = categoryRepository;
        _cache = cache;
    }

    public async Task<ProductDto> GetProductByIdAsync(Guid id)
    {
        var cacheKey = $"{CachePrefix}{id}";
        var cached = await _cache.GetStringAsync(cacheKey);
        
        if (!string.IsNullOrEmpty(cached))
        {
            return JsonSerializer.Deserialize<ProductDto>(cached)!;
        }

        var product = await _productRepository.GetByIdAsync(id);
        if (product == null) return null!;

        var dto = MapToDto(product);
        
        var options = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
        };
        await _cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(dto), options);

        return dto;
    }

    public async Task<IEnumerable<ProductDto>> GetAllProductsAsync()
    {
        var products = await _productRepository.GetAllAsync();
        return products.Select(MapToDto);
    }

    public async Task<IEnumerable<ProductDto>> GetProductsByCategoryAsync(Guid categoryId)
    {
        var allProducts = await _productRepository.GetAllAsync();
        var products = allProducts.Where(p => p.CategoryId == categoryId);
        return products.Select(MapToDto);
    }

    public async Task<ProductDto> CreateProductAsync(CreateProductCommand command)
    {
        var product = new ProductEntity
        {
            Name = command.Name,
            Description = command.Description,
            Price = command.Price,
            StockQuantity = command.StockQuantity,
            CategoryId = command.CategoryId
        };

        await _productRepository.AddAsync(product);
        await _productRepository.SaveChangesAsync();

        return MapToDto(product);
    }

    public async Task UpdateProductAsync(UpdateProductCommand command)
    {
        var product = await _productRepository.GetByIdAsync(command.Id);
        if (product == null) return;

        product.Name = command.Name;
        product.Description = command.Description;
        product.Price = command.Price;
        product.StockQuantity = command.StockQuantity;
        product.CategoryId = command.CategoryId;
        product.UpdatedAt = DateTime.UtcNow;

        await _productRepository.UpdateAsync(product);
        await _productRepository.SaveChangesAsync();

        // Invalidate cache
        await _cache.RemoveAsync($"{CachePrefix}{command.Id}");
    }

    public async Task DeleteProductAsync(Guid id)
    {
        var product = await _productRepository.GetByIdAsync(id);
        if (product == null) return;

        await _productRepository.DeleteAsync(product);
        await _productRepository.SaveChangesAsync();

        // Invalidate cache
        await _cache.RemoveAsync($"{CachePrefix}{id}");
    }

    public async Task UpdateStockAsync(Guid productId, int newStockQuantity)
    {
        var product = await _productRepository.GetByIdAsync(productId);
        if (product == null) return;

        product.StockQuantity = newStockQuantity;
        product.UpdatedAt = DateTime.UtcNow;

        await _productRepository.UpdateAsync(product);
        await _productRepository.SaveChangesAsync();

        // Invalidate cache
        await _cache.RemoveAsync($"{CachePrefix}{productId}");
    }

    private ProductDto MapToDto(ProductEntity product)
    {
        return new ProductDto
        {
            Id = product.Id,
            Name = product.Name,
            Description = product.Description,
            Price = product.Price,
            StockQuantity = product.StockQuantity,
            CategoryId = product.CategoryId,
            CategoryName = product.Category?.Name ?? string.Empty
        };
    }
}

