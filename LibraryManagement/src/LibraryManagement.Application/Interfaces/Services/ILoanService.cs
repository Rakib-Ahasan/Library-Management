using LibraryManagement.Application.Common;
using LibraryManagement.Application.Dtos;

namespace LibraryManagement.Application.Interfaces.Services;

/// <summary>
/// Defines the contract for loan operations.
/// </summary>
public interface ILoanService
{
    /// <summary>
    /// Retrieves a paginated list of loans.
    /// </summary>
    Task<PagedResult<LoanResponse>> GetPagedAsync(PagedQuery query, bool activeOnly, CancellationToken ct = default);

    /// <summary>
    /// Retrieves a list of overdue loans.
    /// </summary>
    Task<IReadOnlyList<LoanResponse>> GetOverdueAsync(CancellationToken ct = default);

    /// <summary>
    /// Retrieves a loan by its identifier.
    /// </summary>
    Task<LoanResponse> GetByIdAsync(int id, CancellationToken ct = default);

    /// <summary>
    /// Borrows a book for a member.
    /// </summary>
    Task<LoanResponse> BorrowAsync(BorrowRequest request, CancellationToken ct = default);

    /// <summary>
    /// Returns a borrowed book.
    /// </summary>
    Task<LoanResponse> ReturnAsync(int loanId, CancellationToken ct = default);
}
