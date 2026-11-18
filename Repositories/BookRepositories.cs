using LibraryManagementSystem.Data;
using LibraryManagementSystem.Dtos;
using LibraryManagementSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagementSystem.Repositories;
public class BookRepository : GenericRepository<Book> , IBookRepository
{
    private readonly AppDbContext _context ;
    public BookRepository (AppDbContext context) : base (context)
    {
        _context =context;

    }
    public async Task<Book?> GetByIdWithCategoryAsync(int id)
    {
        return await _context.Books
            .AsNoTracking()
            .Include(b => b.Category)
            .FirstOrDefaultAsync(b => b.BookId == id && !b.IsDeleted);
    }

    public async Task<PagedResult<Book>> GetPagedAsync(
        BookQueryParams query)
    {
        var pageNumber = query.PageNumber < 1 ? 1 : query.PageNumber;
        var pageSize = query.PageSize < 1
            ? 10
            : Math.Min(query.PageSize, 50);

        var books = _context.Books
            .AsNoTracking()
            .Include(b => b.Category)
            .Where(b => !b.IsDeleted);

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim();

            books = books.Where(b =>
                b.Title.Contains(search) ||
                b.Author.Contains(search));
        }

        if (query.CategoryId.HasValue)
        {
            books = books.Where(b =>
                b.CategoryId == query.CategoryId.Value);
        }

        var totalCount = await books.CountAsync();

        var sortedBooks = query.SortBy?.ToLowerInvariant() switch
        {
            "price" => query.Descending
                ? books.OrderByDescending(b => b.Price)
                : books.OrderBy(b => b.Price),

            "createdat" => query.Descending
                ? books.OrderByDescending(b => b.CreatedAt)
                : books.OrderBy(b => b.CreatedAt),

            _ => query.Descending
                ? books.OrderByDescending(b => b.Title)
                : books.OrderBy(b => b.Title)
        };

        var items = await sortedBooks
            .ThenBy(b => b.BookId)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResult<Book>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }

}