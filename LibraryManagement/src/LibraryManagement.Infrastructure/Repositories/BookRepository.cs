using LibraryManagement.Application.Common;
using LibraryManagement.Application.Interfaces;
using LibraryManagement.Domain.Entities;
using LibraryManagement.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagement.Infrastructure.Repositories;

/// <inheritdoc/>
public class BookRepository : IBookRepository
{
    private readonly ApplicationDbContext _context;

    /// <summary>
    /// Initialises a new instance of <see cref="BookRepository"/>.
    /// </summary>
    public BookRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    /// <inheritdoc/>
    public async Task<Book?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        return await _context.Books.FindAsync(id, ct);
    }

    /// <inheritdoc/>
    public async Task<(IReadOnlyList<Book> Items, int TotalCount)> GetPagedAsync(PagedQuery query, CancellationToken ct = default)
    {
        var queryable = _context.Books.AsNoTracking()
            .Where(b =>
                (string.IsNullOrEmpty(query.Search) || 
                 b.Title.Contains(query.Search) || 
                 b.Author.Contains(query.Search) ||
                 b.Isbn.Contains(query.Search)))
            .OrderBy(b => b.Title)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize);

        var items = await queryable.ToListAsync(ct);
        var totalCount = await _context.Books.CountAsync(book =>
            (string.IsNullOrEmpty(query.Search) ||
             book.Title.Contains(query.Search) ||
             book.Author.Contains(query.Search) ||
             book.Isbn.Contains(query.Search)), ct);

        return (items, totalCount);
    }

    /// <inheritdoc/>
    public async Task<bool> IsbnExistsAsync(string isbn, int? excludeId, CancellationToken ct = default)
    {
        var query = _context.Books.AsNoTracking().Where(b => b.Isbn == isbn);

        if (excludeId.HasValue)
        {
            query = query.Where(b => b.Id != excludeId.Value);
        }

        return await query.AnyAsync(ct);
    }

    /// <inheritdoc/>
    public async Task<bool> HasLoansAsync(int bookId, CancellationToken ct = default)
    {
        return await _context.Loans.AsNoTracking()
            .AnyAsync(l => l.BookId == bookId && l.ReturnedOn == null, ct);
    }

    /// <inheritdoc/>
    public async Task AddAsync(Book book, CancellationToken ct = default)
    {
        await _context.Books.AddAsync(book, ct);
    }

    /// <inheritdoc/>
    public void Remove(Book book)
    {
        _context.Books.Remove(book);
    }
}