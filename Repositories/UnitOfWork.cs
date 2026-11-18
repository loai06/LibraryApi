using LibraryManagementSystem.Data;
using LibraryManagementSystem.Models;

namespace LibraryManagementSystem.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _context;

    public IBookRepository Books { get; }

    public ICategoryRepository Categories { get; }

    public UnitOfWork(AppDbContext context)
    {
        _context = context;

        Books = new BookRepository(context);
        Categories = new CategoryRepository(context);
    }

    public Task<int> SaveChangesAsync()
    {
        return _context.SaveChangesAsync();
    }
}