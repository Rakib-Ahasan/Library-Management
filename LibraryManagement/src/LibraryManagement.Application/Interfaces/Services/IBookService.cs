using LibraryManagement.Application.Common;
using LibraryManagement.Application.Dtos;

namespace LibraryManagement.Application.Interfaces.Services;

/// <summary>
/// Defines the contract for book operations.
/// </summary>
public interface IBookService
{
    /// <summary>
    /// Retrieves a paginated list of books.
    /// </summary>
    Task<PagedResult<BookResponse>> GetPagedAsync(PagedQuery query, CancellationToken ct = default);

    /// <summary>
    /// Retrieves a book by its identifier.
    /// </summary>
    Task<BookResponse> GetByIdAsync(int id, CancellationToken ct = default);

    /// <summary>
    /// Creates a new book.
    /// </summary>
    Task<BookResponse> CreateAsync(BookRequest request, CancellationToken ct = default);

    /// <summary>
    /// Updates an existing book.
    /// </summary>
    Task UpdateAsync(int id, BookRequest request, CancellationToken ct = default);

    /// <summary>
    /// Deletes a book.
    /// </summary>
    Task DeleteAsync(int id, CancellationToken ct = default);
}
