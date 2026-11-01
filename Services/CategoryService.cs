using LibraryManagementSystem.Dtos;
using LibraryManagementSystem.Models;
using LibraryManagementSystem.Data;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagementSystem.Services;

public class CategoryService : ICategoryService
{
    private readonly AppDbContext _context;

    
    public CategoryService(AppDbContext context)
    {
        _context = context;
    }

    //GET ALL 
    public async Task<PagedResult<CategoryResponseDto>> GetAllAsync(CategoryQueryParams query)
    {
        
        var pageNumber = query.PageNumber < 1 ? 1 : query.PageNumber;
        var pageSize = query.PageSize < 1 ? 10 : Math.Min(query.PageSize, 50);

        
        var categories = _context.Categories
            .AsNoTracking()
            .Where(c => !c.IsDeleted);

        
        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim();
            categories = categories.Where(c => c.Name.Contains(search));
        }

        var totalCount = await categories.CountAsync();

        categories = query.SortBy?.ToLower() switch
        {
            "createdat" => query.Descending
                ? categories.OrderByDescending(c => c.CreatedAt)
                : categories.OrderBy(c => c.CreatedAt),
            _ => query.Descending
                ? categories.OrderByDescending(c => c.Name)
                : categories.OrderBy(c => c.Name)
        };

        
        var items = await categories
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        
        return new PagedResult<CategoryResponseDto>
        {
            Items = items.Select(MapToDto).ToList(),
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }

    // GET BY ID
    public async Task<CategoryResponseDto?> GetByIdAsync(int id)
    {
        var category = await _context.Categories
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.CategoryId == id && !c.IsDeleted);

        if (category == null) return null;

        return MapToDto(category);
    }

    //CREATE
    public async Task<CategoryResponseDto> CreateAsync(CategoryCreateDto dto)
    {
        var category = new Category
        {
            Name = dto.Name.Trim(),
            Description = dto.Description ?? string.Empty,
            CreatedAt = DateTime.UtcNow,
            IsDeleted = false
        };

        _context.Categories.Add(category);
        await _context.SaveChangesAsync();

        return MapToDto(category);
    }

    //UPDATE
    public async Task<CategoryResponseDto?> UpdateAsync(int id, CategoryUpdateDto dto)
    {
        var category = await _context.Categories
            .FirstOrDefaultAsync(c => c.CategoryId == id && !c.IsDeleted);

        if (category == null) return null;

        category.Name = dto.Name.Trim();
        category.Description = dto.Description ?? string.Empty;

        await _context.SaveChangesAsync();

        return MapToDto(category);
    }

    //DELETE
    public async Task<bool> DeleteAsync(int id)
    {
        var category = await _context.Categories
            .FirstOrDefaultAsync(c => c.CategoryId == id && !c.IsDeleted);

        if (category == null) return false; 

        
        var hasBooks = await _context.Books
            .AnyAsync(b => b.CategoryId == id && !b.IsDeleted);

        if (hasBooks) return false; 

        category.IsDeleted = true;
        await _context.SaveChangesAsync();
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