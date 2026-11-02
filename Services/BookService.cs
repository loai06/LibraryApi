using LibraryManagementSystem.Dtos;
using LibraryManagementSystem.Models;
using LibraryManagementSystem.Data;
using Microsoft.EntityFrameworkCore;
using LibraryManagementSystem.Exceptions;

namespace LibraryManagementSystem.Services;

public class BookService : IBookService
{
    private readonly AppDbContext _context;
    public BookService(AppDbContext context){
         _context = context;
         }

    public async Task<PagedResult<BookResponseDto>> GetAllAsync(BookQueryParams query){
    
        var pageNumber = query.PageNumber < 1 ? 1 : query.PageNumber;
        var pageSize = query.PageSize < 1 ? 10 : Math.Min(query.PageSize, 50);

        var books = _context.Books
            .AsNoTracking()
            .Include(b => b.Category)
            .Where(b => !b.IsDeleted);

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim();
            books = books.Where(b => b.Title.Contains(search) || b.Author.Contains(search));
        }

        if (query.CategoryId.HasValue)
        {
            books = books.Where(b => b.CategoryId == query.CategoryId.Value);
        }

        var totalCount = await books.CountAsync();

        books = query.SortBy?.ToLower() switch
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

        var items = await books
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResult<BookResponseDto>
        {
            Items = items.Select(MapToDto).ToList(),
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }

    public async Task<BookResponseDto?> GetByIdAsync(int id)
    {
        var book = await _context.Books
            .AsNoTracking()
            .Include(b => b.Category)
            .FirstOrDefaultAsync(b => b.BookId == id && !b.IsDeleted);
        if (book == null) 
        throw new NotFoundException($"Book with ID {id} not found.");

        return MapToDto(book);
    }
    public async Task<BookResponseDto> CreateAsync(BookCreateDto dto)
    {
        var category = await _context.Categories.FirstOrDefaultAsync(c => c.CategoryId == dto.CategoryId);
        if (category == null)
        {
            throw new BadRequestException($"Category with ID {dto.CategoryId} does not exist.");
        }
        var book = new Book
        {
            Title = dto.Title.Trim(),
            Author = dto.Author.Trim(),
            Price = dto.Price,
            Quantity = dto.Quantity,
            CategoryId = dto.CategoryId,
            Category = category,
        };

        _context.Books.Add(book);
        await _context.SaveChangesAsync();

        return MapToDto(book);
    }
    public async Task<BookResponseDto?> UpdateAsync(int id, BookUpdateDto dto)
    {

        var category = await _context.Categories.FirstOrDefaultAsync(c => c.CategoryId == dto.CategoryId);
        if (category == null)
        {
            throw new BadRequestException($"Category with ID {dto.CategoryId} does not exist.");
        }
        var book = await _context.Books
            .FirstOrDefaultAsync(b => b.BookId == id && !b.IsDeleted);

        if (book == null) 
        throw new NotFoundException($"Book with ID {id} not found.");

        book.Title = dto.Title.Trim();
        book.Author = dto.Author.Trim();
        book.Price = dto.Price;
        book.Quantity = dto.Quantity;
        book.CategoryId = dto.CategoryId;
        book.Category = category;

        await _context.SaveChangesAsync();

        return MapToDto(book);
    }
    public async Task<bool> DeleteAsync(int id)
    {
        var book = await _context.Books
            .FirstOrDefaultAsync(b => b.BookId == id && !b.IsDeleted);

        if (book == null) 
        throw new NotFoundException($"Book with ID {id} not found.");

        book.IsDeleted = true;
        await _context.SaveChangesAsync();
        return true;
    }


private static BookResponseDto MapToDto(Book book)
{
    return new BookResponseDto
    {
        Id = book.BookId,
        Title = book.Title,
        Author = book.Author,
        Price = book.Price,
        Quantity = book.Quantity,
        CategoryId = book.CategoryId,
        CategoryName = book.Category?.Name ?? string.Empty,
        CreatedAt = book.CreatedAt
    };
}
   }   


