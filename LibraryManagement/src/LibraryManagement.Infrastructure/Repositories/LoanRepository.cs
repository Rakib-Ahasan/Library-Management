using LibraryManagement.Application.Common;
using LibraryManagement.Application.Interfaces;
using LibraryManagement.Domain.Entities;
using LibraryManagement.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagement.Infrastructure.Repositories;

/// <inheritdoc/>
public class LoanRepository : ILoanRepository
{
    private readonly ApplicationDbContext _context;

    /// <summary>
    /// Initialises a new instance of <see cref="LoanRepository"/>.
    /// </summary>
    public LoanRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    /// <inheritdoc/>
    public async Task<Loan?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        return await _context.Loans.FindAsync(id, ct);
    }

    /// <inheritdoc/>
    public async Task<(IReadOnlyList<Loan> Items, int TotalCount)> GetPagedAsync(PagedQuery query, bool activeOnly, CancellationToken ct = default)
    {
        var queryable = _context.Loans.AsNoTracking()
            .Where(l =>
                activeOnly ? l.ReturnedOn == null : true)
            .OrderBy(l => l.BorrowedOn)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize);

        var items = await queryable.ToListAsync(ct);
        var totalCount = activeOnly
            ? await _context.Loans.CountAsync(l => l.ReturnedOn == null, ct)
            : await _context.Loans.CountAsync(ct);

        return (items, totalCount);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<Loan>> GetOverdueAsync(DateTime now, CancellationToken ct = default)
    {
        return await _context.Loans.AsNoTracking()
            .Where(l => l.ReturnedOn == null && l.DueDate < now)
            .OrderBy(l => l.DueDate)
            .ToListAsync(ct);
    }

    /// <inheritdoc/>
    public async Task<int> CountActiveByMemberAsync(int memberId, CancellationToken ct = default)
    {
        return await _context.Loans.AsNoTracking()
            .CountAsync(l => l.MemberId == memberId && l.ReturnedOn == null, ct);
    }

    /// <inheritdoc/>
    public async Task AddAsync(Loan loan, CancellationToken ct = default)
    {
        await _context.Loans.AddAsync(loan, ct);
    }

    /// <inheritdoc/>
    public void Remove(Loan loan)
    {
        _context.Loans.Remove(loan);
    }
}