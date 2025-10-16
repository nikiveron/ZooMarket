using Catalog.Application.DTOs;
using MediatR;

namespace Catalog.Application.Features.Products;

public record CreateProductCommand(
    string Name,
    string Description,
    decimal Price,
    int StockQuantity,
    Guid CategoryId) : IRequest<ProductDto>;

public record UpdateProductCommand(
    Guid Id,
    string Name,
    string Description,
    decimal Price,
    int StockQuantity,
    Guid CategoryId) : IRequest<Unit>;

public record UpdateProductStockCommand(
    Guid ProductId,
    int NewStockQuantity) : IRequest<Unit>;
