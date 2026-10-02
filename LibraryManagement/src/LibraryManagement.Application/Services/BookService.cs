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
/// Implements the <see cref="IBookService"/> contract.
/// </summary>
public class BookService : IBookService
{
    private readonly IBookRepository _repository;
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;
    private readonly IValidator<BookRequest> _validator;

    /// <summary>
    /// Initialises a new instance of <see cref="BookService"/>.
    /// </summary>
    public BookService(
        IBookRepository repository,
        IUnitOfWork uow,
        IMapper mapper,
        IValidator<BookRequest> validator)
    {
        _repository = repository;
        _uow = uow;
        _mapper = mapper;
        _validator = validator;
    }

    /// <inheritdoc/>
    public async Task<PagedResult<BookResponse>> GetPagedAsync(PagedQuery query, CancellationToken ct = default)
    {
        var (books, total) = await _repository.GetPagedAsync(query, ct);
        var items = _mapper.Map<IEnumerable<BookResponse>>(books);
        return new PagedResult<BookResponse>(items, query.Page, query.PageSize, total);
    }

    /// <inheritdoc/>
    public async Task<BookResponse> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var book = await _repository.GetByIdAsync(id, ct);
        return book == null
            ? throw new NotFoundException($"Book with identifier {id} was not found.")
            : _mapper.Map<BookResponse>(book);
    }

    /// <inheritdoc/>
    public async Task<BookResponse> CreateAsync(BookRequest request, CancellationToken ct = default)
    {
        var validationResult = await _validator.ValidateAsync(request, ct);
        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);

        if (await _repository.IsbnExistsAsync(request.Isbn, null, ct))
            throw new ConflictException($"A book with ISBN '{request.Isbn}' already exists.");

        var book = _mapper.Map<Book>(request);
        await _repository.AddAsync(book, ct);
        await _uow.SaveChangesAsync(ct);
        return _mapper.Map<BookResponse>(book);
    }

    /// <inheritdoc/>
    public async Task UpdateAsync(int id, BookRequest request, CancellationToken ct = default)
    {
        var existing = await _repository.GetByIdAsync(id, ct)
            ?? throw new NotFoundException($"Book with identifier {id} was not found.");

        var validationResult = await _validator.ValidateAsync(request, ct);
        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);

        if (await _repository.IsbnExistsAsync(request.Isbn, id, ct))
            throw new ConflictException($"A book with ISBN '{request.Isbn}' already exists.");

        _mapper.Map(request, existing);
        await _uow.SaveChangesAsync(ct);
    }

    /// <inheritdoc/>
    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        var book = await _repository.GetByIdAsync(id, ct)
            ?? throw new NotFoundException($"Book with identifier {id} was not found.");

        if (await _repository.HasLoansAsync(id, ct))
            throw new BusinessRuleException(
                "Cannot delete a book that has active loans. Return all copies first.");

        _repository.Remove(book);
        await _uow.SaveChangesAsync(ct);
    }
}
