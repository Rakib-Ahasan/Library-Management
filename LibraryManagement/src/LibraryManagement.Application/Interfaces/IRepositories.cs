using LibraryManagement.Application.Common;
using LibraryManagement.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace LibraryManagement.Application.Interfaces
{
    public interface IBookRepository
    {
        Task<Book?> GetByIdAsync(int id, CancellationToken ct = default);
        Task<(IReadOnlyList<Book> Items, int TotalCount)> GetPagedAsync(PagedQuery query, CancellationToken ct = default);
        Task<bool> IsbnExistsAsync(string isbn, int? excludeId, CancellationToken ct = default);
        Task<bool> HasLoansAsync(int bookId, CancellationToken ct = default);
        Task AddAsync(Book book, CancellationToken ct = default);
        void Remove(Book book);
    }

    public interface IMemberRepository
    {
        Task<Member?> GetByIdAsync(int id, CancellationToken ct = default);
        Task<(IReadOnlyList<Member> Items, int TotalCount)> GetPagedAsync(PagedQuery query, CancellationToken ct = default);
        Task<bool> EmailExistsAsync(string email, int? excludeId, CancellationToken ct = default);
        Task<bool> HasLoansAsync(int memberId, CancellationToken ct = default);
        Task AddAsync(Member member, CancellationToken ct = default);
        void Remove(Member member);
    }

    public interface ILoanRepository
    {
        Task<Loan?> GetByIdAsync(int id, CancellationToken ct = default);
        Task<(IReadOnlyList<Loan> Items, int TotalCount)> GetPagedAsync(PagedQuery query, bool activeOnly, CancellationToken ct = default);
        Task<IReadOnlyList<Loan>> GetOverdueAsync(DateTime now, CancellationToken ct = default);
        Task<int> CountActiveByMemberAsync(int memberId, CancellationToken ct = default);
        Task AddAsync(Loan loan, CancellationToken ct = default);
    }

    public interface IUnitOfWork
    {
        Task<int> SaveChangesAsync(CancellationToken ct = default);
    }
}
