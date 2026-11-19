using AutoMapper;
using LibraryManagementSystem.Dtos;
using LibraryManagementSystem.Exceptions;
using LibraryManagementSystem.Models;
using LibraryManagementSystem.Repositories;

namespace LibraryManagementSystem.Services;

public class CategoryServices : ICategoryService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public CategoryServices(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<PagedResult<CategoryResponseDto>> GetAllAsync(
        CategoryQueryParams query)
    {
        var result = await _unitOfWork.Categories.GetPagedAsync(query);

        return new PagedResult<CategoryResponseDto>
        {
            Items = _mapper.Map<List<CategoryResponseDto>>(result.Items),
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

        return _mapper.Map<CategoryResponseDto>(category);
    }

    public async Task<CategoryResponseDto> CreateAsync(
        CategoryCreateDto dto)
    {
        var category = _mapper.Map<Category>(dto);
        category.CreatedAt = DateTime.UtcNow;
        category.IsDeleted = false;

        await _unitOfWork.Categories.AddAsync(category);
        await _unitOfWork.SaveChangesAsync();

        return _mapper.Map<CategoryResponseDto>(category);
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

        _mapper.Map(dto, category);

        await _unitOfWork.SaveChangesAsync();

        return _mapper.Map<CategoryResponseDto>(category);
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
}