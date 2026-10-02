using LibraryManagement.Application.Common;
using LibraryManagement.Application.Interfaces;
using LibraryManagement.Domain.Entities;
using LibraryManagement.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagement.Infrastructure.Repositories;

public class MemberRepository : IMemberRepository
{
    private readonly ApplicationDbContext _context;

    public MemberRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Member?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        return await _context.Members.FindAsync(id, ct);
    }

    public async Task<(IReadOnlyList<Member> Items, int TotalCount)> GetPagedAsync(PagedQuery query, CancellationToken ct = default)
    {
        var queryable = _context.Members.AsNoTracking()
            .Where(m =>
                (string.IsNullOrEmpty(query.Search) ||
                 m.FullName.Contains(query.Search) ||
                 m.Email.Contains(query.Search)))
            .OrderBy(m => m.FullName)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize);

        var items = await queryable.ToListAsync(ct);
        var totalCount = await _context.Members.CountAsync(m =>
            (string.IsNullOrEmpty(query.Search) ||
             m.FullName.Contains(query.Search) ||
             m.Email.Contains(query.Search)), ct);

        return (items, totalCount);
    }

    public async Task<bool> EmailExistsAsync(string email, int? excludeId, CancellationToken ct = default)
    {
        var query = _context.Members.AsNoTracking().Where(m => m.Email == email);

        if (excludeId.HasValue)
        {
            query = query.Where(m => m.Id != excludeId.Value);
        }

        return await query.AnyAsync(ct);
    }

    public async Task<bool> HasLoansAsync(int memberId, CancellationToken ct = default)
    {
        return await _context.Loans.AsNoTracking()
            .AnyAsync(l => l.MemberId == memberId && l.ReturnedOn == null, ct);
    }

    public async Task AddAsync(Member member, CancellationToken ct = default)
    {
        await _context.Members.AddAsync(member, ct);
    }

    public void Remove(Member member)
    {
        _context.Members.Remove(member);
    }
}
