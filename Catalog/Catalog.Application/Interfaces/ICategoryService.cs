using Catalog.Application.DTOs;
using Catalog.Application.Features.Categories;

namespace Catalog.Application.Interfaces;

public interface ICategoryService
{
    Task<CategoryDto> GetCategoryByIdAsync(Guid id);
    Task<IEnumerable<CategoryDto>> GetAllCategoriesAsync();
    Task<CategoryDto> CreateCategoryAsync(CreateCategoryCommand command);
    Task UpdateCategoryAsync(UpdateCategoryCommand command);
    Task DeleteCategoryAsync(Guid id);
}
