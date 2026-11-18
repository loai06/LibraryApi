using LibraryManagementSystem.Dtos;
using LibraryManagementSystem.Models;

namespace LibraryManagementSystem.Repositories;

public interface IBookRepository : IGenericRepository<Book>
{
    Task<Book?> GetByIdWithCategoryAsync(int id);

    Task<PagedResult<Book>> GetPagedAsync(BookQueryParams query);
}