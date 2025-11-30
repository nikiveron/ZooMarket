using Catalog.Application.Interfaces;
using MediatR;

namespace Catalog.Application.Features.Products;

public class UpdateProductCommandHandler : IRequestHandler<UpdateProductCommand, Unit>
{
    private readonly IProductService _productService;

    public UpdateProductCommandHandler(IProductService productService)
    {
        _productService = productService;
    }

    public async Task<Unit> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
    {
        await _productService.UpdateProductAsync(request);
        return Unit.Value;
    }
}

