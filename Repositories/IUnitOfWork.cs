using LibraryManagementSystem.Models;

namespace LibraryManagementSystem.Repositories;

public interface IUnitOfWork
{
    IBookRepository Books { get; }

    ICategoryRepository Categories { get; }

    Task<int> SaveChangesAsync();
}