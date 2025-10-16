using Catalog.Application.DTOs;
using MediatR;

namespace Catalog.Application.Features.Categories;

public record CreateCategoryCommand(
    string Name,
    string Description) : IRequest<CategoryDto>;

public record UpdateCategoryCommand(
    Guid Id,
    string Name,
    string Description) : IRequest<Unit>;
