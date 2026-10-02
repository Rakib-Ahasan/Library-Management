using AutoMapper;
using FluentValidation;
using LibraryManagement.Application.Common;
using LibraryManagement.Application.Dtos;
using LibraryManagement.Application.Interfaces;
using LibraryManagement.Application.Interfaces.Services;
using LibraryManagement.Domain.Entities;
using LibraryManagement.Domain.Exceptions;

namespace LibraryManagement.Application.Services;

public class MemberService : IMemberService
{
    private readonly IMemberRepository _repository;
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;
    private readonly IValidator<MemberRequest> _validator;

    public MemberService(
        IMemberRepository repository,
        IUnitOfWork uow,
        IMapper mapper,
        IValidator<MemberRequest> validator)
    {
        _repository = repository;
        _uow = uow;
        _mapper = mapper;
        _validator = validator;
    }

    public async Task<PagedResult<MemberResponse>> GetPagedAsync(PagedQuery query, CancellationToken ct = default)
    {
        var (members, total) = await _repository.GetPagedAsync(query, ct);
        var items = _mapper.Map<IEnumerable<MemberResponse>>(members);
        return new PagedResult<MemberResponse>(items, query.Page, query.PageSize, total);
    }

    public async Task<MemberResponse> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var member = await _repository.GetByIdAsync(id, ct);
        return member == null
            ? throw new NotFoundException($"Member with identifier {id} was not found.")
            : _mapper.Map<MemberResponse>(member);
    }

    public async Task<MemberResponse> CreateAsync(MemberRequest request, CancellationToken ct = default)
    {
        var validationResult = await _validator.ValidateAsync(request, ct);
        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);

        if (await _repository.EmailExistsAsync(request.Email, null, ct))
            throw new ConflictException($"A member with email '{request.Email}' already exists.");

        var member = _mapper.Map<Member>(request);
        member.JoinedOn = DateTime.UtcNow;
        await _repository.AddAsync(member, ct);
        await _uow.SaveChangesAsync(ct);
        return _mapper.Map<MemberResponse>(member);
    }

    public async Task UpdateAsync(int id, MemberRequest request, CancellationToken ct = default)
    {
        var existing = await _repository.GetByIdAsync(id, ct)
            ?? throw new NotFoundException($"Member with identifier {id} was not found.");

        var validationResult = await _validator.ValidateAsync(request, ct);
        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);

        if (await _repository.EmailExistsAsync(request.Email, id, ct))
            throw new ConflictException($"A member with email '{request.Email}' already exists.");

        _mapper.Map(request, existing);
        await _uow.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        var member = await _repository.GetByIdAsync(id, ct)
            ?? throw new NotFoundException($"Member with identifier {id} was not found.");

        if (await _repository.HasLoansAsync(id, ct))
            throw new BusinessRuleException(
                "Cannot delete a member that has active loans. Return all books first.");

        _repository.Remove(member);
        await _uow.SaveChangesAsync(ct);
    }
}
