using AutoMapper;
using LibraryManagementSystem.Dtos;
using LibraryManagementSystem.Exceptions;
using LibraryManagementSystem.Models;
using LibraryManagementSystem.Repositories;

namespace LibraryManagementSystem.Services;

public class BookServices : IBookService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public BookServices(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<PagedResult<BookResponseDto>> GetAllAsync(
        BookQueryParams query)
    {
        var result = await _unitOfWork.Books.GetPagedAsync(query);

        return new PagedResult<BookResponseDto>
        {
            Items = _mapper.Map<List<BookResponseDto>>(result.Items),
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

        return _mapper.Map<BookResponseDto>(book);
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

        var book = _mapper.Map<Book>(dto);
        book.Category = category;

        await _unitOfWork.Books.AddAsync(book);
        await _unitOfWork.SaveChangesAsync();

        return _mapper.Map<BookResponseDto>(book);
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

        _mapper.Map(dto, book);
        book.Category = category;

        await _unitOfWork.SaveChangesAsync();

        return _mapper.Map<BookResponseDto>(book);
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
}