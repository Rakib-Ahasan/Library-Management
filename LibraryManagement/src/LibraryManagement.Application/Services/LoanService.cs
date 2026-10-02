using AutoMapper;
using FluentValidation;
using LibraryManagement.Application.Common;
using LibraryManagement.Application.Dtos;
using LibraryManagement.Application.Interfaces;
using LibraryManagement.Application.Interfaces.Services;
using LibraryManagement.Domain.Entities;
using LibraryManagement.Domain.Exceptions;

namespace LibraryManagement.Application.Services;

/// <summary>
/// Implements the <see cref="ILoanService"/> contract.
/// </summary>
public class LoanService : ILoanService
{
    private readonly ILoanRepository _repository;
    private readonly IBookRepository _bookRepository;
    private readonly IMemberRepository _memberRepository;
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;
    private readonly IValidator<BorrowRequest> _validator;

    /// <summary>
    /// Initialises a new instance of <see cref="LoanService"/>.
    /// </summary>
    public LoanService(
        ILoanRepository repository,
        IBookRepository bookRepository,
        IMemberRepository memberRepository,
        IUnitOfWork uow,
        IMapper mapper,
        IValidator<BorrowRequest> validator)
    {
        _repository = repository;
        _bookRepository = bookRepository;
        _memberRepository = memberRepository;
        _uow = uow;
        _mapper = mapper;
        _validator = validator;
    }

    /// <inheritdoc/>
    public async Task<PagedResult<LoanResponse>> GetPagedAsync(PagedQuery query, bool activeOnly, CancellationToken ct = default)
    {
        var (loans, total) = await _repository.GetPagedAsync(query, activeOnly, ct);
        var items = _mapper.Map<IEnumerable<LoanResponse>>(loans);
        return new PagedResult<LoanResponse>(items, query.Page, query.PageSize, total);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<LoanResponse>> GetOverdueAsync(CancellationToken ct = default)
    {
        var loans = await _repository.GetOverdueAsync(DateTime.UtcNow, ct);
        return _mapper.Map<IReadOnlyList<LoanResponse>>(loans);
    }

    /// <inheritdoc/>
    public async Task<LoanResponse> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var loan = await _repository.GetByIdAsync(id, ct);
        return loan == null
            ? throw new NotFoundException($"Loan with identifier {id} was not found.")
            : _mapper.Map<LoanResponse>(loan);
    }

    /// <inheritdoc/>
    public async Task<LoanResponse> BorrowAsync(BorrowRequest request, CancellationToken ct = default)
    {
        var validationResult = await _validator.ValidateAsync(request, ct);
        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);

        var book = await _bookRepository.GetByIdAsync(request.BookId, ct)
            ?? throw new NotFoundException($"Book with identifier {request.BookId} was not found.");
        var member = await _memberRepository.GetByIdAsync(request.MemberId, ct)
            ?? throw new NotFoundException($"Member with identifier {request.MemberId} was not found.");

        const int MaxActiveLoans = 5;
        var activeCount = await _repository.CountActiveByMemberAsync(request.MemberId, ct);
        if (activeCount >= MaxActiveLoans)
            throw new BusinessRuleException(
                $"Member already has {activeCount} active loans. Maximum allowed is {MaxActiveLoans}.");

        if (book.AvailableCopies < 1)
            throw new BusinessRuleException("No copies of this book are available for borrowing.");

        book.TakeCopy();
        var loan = new Loan
        {
            BookId = request.BookId,
            MemberId = request.MemberId,
            BorrowedOn = DateTime.UtcNow,
            DueDate = DateTime.UtcNow.AddDays(request.Days)
        };
        await _repository.AddAsync(loan, ct);
        await _uow.SaveChangesAsync(ct);
        return _mapper.Map<LoanResponse>(loan);
    }

    /// <inheritdoc/>
    public async Task<LoanResponse> ReturnAsync(int loanId, CancellationToken ct = default)
    {
        var loan = await _repository.GetByIdAsync(loanId, ct)
            ?? throw new NotFoundException($"Loan with identifier {loanId} was not found.");

        if (loan.ReturnedOn.HasValue)
            throw new BusinessRuleException("This loan has already been returned.");

        loan.MarkReturned(DateTime.UtcNow);
        await _uow.SaveChangesAsync(ct);
        return _mapper.Map<LoanResponse>(loan);
    }
}
