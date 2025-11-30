using Catalog.Application.Interfaces;
using MediatR;

namespace Catalog.Application.Features.Products;

public class UpdateProductStockCommandHandler : IRequestHandler<UpdateProductStockCommand, Unit>
{
    private readonly IProductService _productService;

    public UpdateProductStockCommandHandler(IProductService productService)
    {
        _productService = productService;
    }

    public async Task<Unit> Handle(UpdateProductStockCommand request, CancellationToken cancellationToken)
    {
        await _productService.UpdateStockAsync(request.ProductId, request.NewStockQuantity);
        return Unit.Value;
    }
}

