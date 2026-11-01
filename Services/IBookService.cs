using LibraryManagementSystem.Dtos;
namespace LibraryManagementSystem.Services
{
    public interface IBookService
    {
        Task<PagedResult<BookResponseDto>> GetAllAsync(BookQueryParams query);
        Task<BookResponseDto?> GetByIdAsync(int id);
        Task<BookResponseDto> CreateAsync(BookCreateDto dto);
        Task<BookResponseDto?> UpdateAsync(int id, BookUpdateDto dto);
        Task <bool> DeleteAsync(int id);
    }
}