using LibraryManagementSystem.Dtos;

namespace LibraryManagementSystem.Services;

public interface ICategoryService
{
    Task<PagedResult<CategoryResponseDto>> GetAllAsync(CategoryQueryParams query);
    Task<CategoryResponseDto?> GetByIdAsync(int id);
    Task<CategoryResponseDto> CreateAsync(CategoryCreateDto dto);
    Task<CategoryResponseDto?> UpdateAsync(int id, CategoryUpdateDto dto);
    Task<bool> DeleteAsync(int id);
}