using LibraryManagementSystem.Dtos;
using LibraryManagementSystem.Models;

namespace LibraryManagementSystem.Repositories;

public interface ICategoryRepository : IGenericRepository<Category>
{
    Task<PagedResult<Category>> GetPagedAsync(CategoryQueryParams query);

    Task<bool> HasActiveBooksAsync(int categoryId);
}