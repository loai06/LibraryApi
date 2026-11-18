using LibraryManagementSystem.Dtos;
using LibraryManagementSystem.Exceptions;
using LibraryManagementSystem.Models;
using LibraryManagementSystem.Repositories;

namespace LibraryManagementSystem.Services;

public class BookServices : IBookService
{
    private readonly IUnitOfWork _unitOfWork;

    public BookServices(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<PagedResult<BookResponseDto>> GetAllAsync(
        BookQueryParams query)
    {
        var result = await _unitOfWork.Books.GetPagedAsync(query);

        return new PagedResult<BookResponseDto>
        {
            Items = result.Items.Select(MapToDto).ToList(),
            TotalCount = result.TotalCount,
            PageNumber = result.PageNumber,
            PageSize = result.PageSize
        };
    }

    public async Task<BookResponseDto?> GetByIdAsync(int id)
    {
        var book = await _unitOfWork.Books
            .GetByIdWithCategoryAsync(id);

        if (book == null)
        {
            throw new NotFoundException(
                $"Book with ID {id} not found.");
        }

        return MapToDto(book);
    }

    public async Task<BookResponseDto> CreateAsync(BookCreateDto dto)
    {
        var category = await _unitOfWork.Categories
            .GetByIdAsync(dto.CategoryId);

        if (category == null)
        {
            throw new BadRequestException(
                $"Category with ID {dto.CategoryId} does not exist.");
        }

        var book = new Book
        {
            Title = dto.Title.Trim(),
            Author = dto.Author.Trim(),
            Price = dto.Price,
            Quantity = dto.Quantity,
            CategoryId = dto.CategoryId,
            Category = category
        };

        await _unitOfWork.Books.AddAsync(book);
        await _unitOfWork.SaveChangesAsync();

        return MapToDto(book);
    }

    public async Task<BookResponseDto?> UpdateAsync(
        int id, BookUpdateDto dto)
    {
        var category = await _unitOfWork.Categories
            .GetByIdAsync(dto.CategoryId);

        if (category == null)
        {
            throw new BadRequestException(
                $"Category with ID {dto.CategoryId} does not exist.");
        }

        var book = await _unitOfWork.Books.GetByIdAsync(id);

        if (book == null || book.IsDeleted)
        {
            throw new NotFoundException(
                $"Book with ID {id} not found.");
        }

        book.Title = dto.Title.Trim();
        book.Author = dto.Author.Trim();
        book.Price = dto.Price;
        book.Quantity = dto.Quantity;
        book.CategoryId = dto.CategoryId;
        book.Category = category;

        await _unitOfWork.SaveChangesAsync();

        return MapToDto(book);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var book = await _unitOfWork.Books.GetByIdAsync(id);

        if (book == null || book.IsDeleted)
        {
            throw new NotFoundException(
                $"Book with ID {id} not found.");
        }

        book.IsDeleted = true;

        await _unitOfWork.SaveChangesAsync();

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