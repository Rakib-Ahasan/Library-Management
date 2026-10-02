using AutoMapper;
using FluentAssertions;
using FluentValidation;
using LibraryManagement.Application.Common.Mappings;
using LibraryManagement.Application.Dtos;
using LibraryManagement.Application.Interfaces;
using LibraryManagement.Application.Services;
using LibraryManagement.Application.Validators;
using LibraryManagement.Domain.Entities;
using LibraryManagement.Domain.Exceptions;
using Moq;
using Microsoft.Extensions.Logging.Abstractions;

namespace LibraryManagement.Tests;

public class LoanServiceTests
{
    private readonly Mock<ILoanRepository> _loanRepository = new();
    private readonly Mock<IBookRepository> _bookRepository = new();
    private readonly Mock<IMemberRepository> _memberRepository = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly IMapper _mapper;
    private readonly IValidator<BorrowRequest> _validator = new BorrowRequestValidator();

    public LoanServiceTests()
    {
        var configuration = new MapperConfiguration(configuration =>
            configuration.AddProfile<LibraryMappingProfile>(), NullLoggerFactory.Instance);
        _mapper = configuration.CreateMapper();
    }

    private LoanService CreateService() => new(
        _loanRepository.Object,
        _bookRepository.Object,
        _memberRepository.Object,
        _unitOfWork.Object,
        _mapper,
        _validator);

    private static Book CreateBook(int availableCopies = 2) => new()
    {
        Id = 1,
        Title = "Clean Code",
        TotalCopies = 2,
        AvailableCopies = availableCopies
    };

    private static Member CreateMember() => new() { Id = 1, FullName = "Ada Lovelace" };

    [Fact]
    public async Task BorrowAsync_DecrementsAvailableCopies()
    {
        var book = CreateBook();
        _bookRepository.Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(book);
        _memberRepository.Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(CreateMember());
        _loanRepository.Setup(x => x.CountActiveByMemberAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(0);
        _loanRepository.Setup(x => x.AddAsync(It.IsAny<Loan>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        await CreateService().BorrowAsync(new BorrowRequest(1, 1));

        book.AvailableCopies.Should().Be(1);
        _unitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task BorrowAsync_ThrowsBusinessRule_WhenNoCopies()
    {
        _bookRepository.Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(CreateBook(0));
        _memberRepository.Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(CreateMember());
        _loanRepository.Setup(x => x.CountActiveByMemberAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(0);

        var action = () => CreateService().BorrowAsync(new BorrowRequest(1, 1));

        await action.Should().ThrowAsync<BusinessRuleException>();
    }

    [Fact]
    public async Task ReturnAsync_MarksReturnedAndIncrementsCopies()
    {
        var book = CreateBook(0);
        var loan = new Loan { Id = 1, Book = book, Member = CreateMember(), BookId = 1, MemberId = 1 };
        _loanRepository.Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(loan);

        await CreateService().ReturnAsync(1);

        loan.ReturnedOn.Should().NotBeNull();
        book.AvailableCopies.Should().Be(1);
    }

    [Fact]
    public async Task ReturnAsync_ThrowsBusinessRule_WhenAlreadyReturned()
    {
        var loan = new Loan { Id = 1, ReturnedOn = DateTime.UtcNow, Book = CreateBook(0) };
        _loanRepository.Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(loan);

        var action = () => CreateService().ReturnAsync(1);

        await action.Should().ThrowAsync<BusinessRuleException>();
    }

    [Fact]
    public async Task GetOverdueAsync_ReturnsOnlyOverdue()
    {
        var overdue = new Loan
        {
            Id = 1,
            DueDate = DateTime.UtcNow.AddDays(-1),
            Book = CreateBook(),
            Member = CreateMember()
        };
        _loanRepository.Setup(x => x.GetOverdueAsync(It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { overdue });

        var result = await CreateService().GetOverdueAsync();

        result.Should().ContainSingle().Which.Id.Should().Be(1);
    }
}
