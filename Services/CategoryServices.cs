using LibraryManagementSystem.Dtos;
using LibraryManagementSystem.Exceptions;
using LibraryManagementSystem.Models;
using LibraryManagementSystem.Repositories;

namespace LibraryManagementSystem.Services;

public class CategoryServices : ICategoryService
{
    private readonly IUnitOfWork _unitOfWork;

    public CategoryServices(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<PagedResult<CategoryResponseDto>> GetAllAsync(
        CategoryQueryParams query)
    {
        var result = await _unitOfWork.Categories.GetPagedAsync(query);

        return new PagedResult<CategoryResponseDto>
        {
            Items = result.Items.Select(MapToDto).ToList(),
            TotalCount = result.TotalCount,
            PageNumber = result.PageNumber,
            PageSize = result.PageSize
        };
    }

    public async Task<CategoryResponseDto?> GetByIdAsync(int id)
    {
        var category = await _unitOfWork.Categories.GetByIdAsync(id);

        if (category == null || category.IsDeleted)
        {
            throw new NotFoundException(
                $"Category with ID {id} not found.");
        }

        return MapToDto(category);
    }

    public async Task<CategoryResponseDto> CreateAsync(
        CategoryCreateDto dto)
    {
        var category = new Category
        {
            Name = dto.Name.Trim(),
            Description = dto.Description ?? string.Empty,
            CreatedAt = DateTime.UtcNow,
            IsDeleted = false
        };

        await _unitOfWork.Categories.AddAsync(category);
        await _unitOfWork.SaveChangesAsync();

        return MapToDto(category);
    }

    public async Task<CategoryResponseDto?> UpdateAsync(
        int id, CategoryUpdateDto dto)
    {
        var category = await _unitOfWork.Categories.GetByIdAsync(id);

        if (category == null || category.IsDeleted)
        {
            throw new NotFoundException(
                $"Category with ID {id} not found.");
        }

        category.Name = dto.Name.Trim();
        category.Description = dto.Description ?? string.Empty;

        await _unitOfWork.SaveChangesAsync();

        return MapToDto(category);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var category = await _unitOfWork.Categories.GetByIdAsync(id);

        if (category == null || category.IsDeleted)
        {
            throw new NotFoundException(
                $"Category with ID {id} not found.");
        }

        var hasBooks = await _unitOfWork.Categories
            .HasActiveBooksAsync(id);

        if (hasBooks)
        {
            throw new BadRequestException(
                $"Cannot delete category with ID {id} because it has associated books.");
        }

        category.IsDeleted = true;

        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    private static CategoryResponseDto MapToDto(Category category)
    {
        return new CategoryResponseDto
        {
            Id = category.CategoryId,
            Name = category.Name,
            Description = category.Description,
            CreatedAt = category.CreatedAt
        };
    }
}