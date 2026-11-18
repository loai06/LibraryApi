using LibraryManagementSystem.Data;
using LibraryManagementSystem.Dtos;
using LibraryManagementSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagementSystem.Repositories;

public class CategoryRepository
    : GenericRepository<Category>, ICategoryRepository
{
    private readonly AppDbContext _context;

    public CategoryRepository(AppDbContext context) : base(context)
    {
        _context = context;
    }

    public async Task<PagedResult<Category>> GetPagedAsync(
        CategoryQueryParams query)
    {
        var pageNumber = query.PageNumber < 1 ? 1 : query.PageNumber;
        var pageSize = query.PageSize < 1
            ? 10
            : Math.Min(query.PageSize, 50);

        var categories = _context.Categories
            .AsNoTracking()
            .Where(c => !c.IsDeleted);

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim();

            categories = categories.Where(c =>
                c.Name.Contains(search));
        }

        var totalCount = await categories.CountAsync();

        var sortedCategories = query.SortBy?.ToLowerInvariant() switch
        {
            "createdat" => query.Descending
                ? categories.OrderByDescending(c => c.CreatedAt)
                : categories.OrderBy(c => c.CreatedAt),

            _ => query.Descending
                ? categories.OrderByDescending(c => c.Name)
                : categories.OrderBy(c => c.Name)
        };

        var items = await sortedCategories
            .ThenBy(c => c.CategoryId)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResult<Category>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }

    public async Task<bool> HasActiveBooksAsync(int categoryId)
    {
        return await _context.Books.AnyAsync(b =>
            b.CategoryId == categoryId && !b.IsDeleted);
    }
}