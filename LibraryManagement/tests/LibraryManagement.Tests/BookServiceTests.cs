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

public class BookServiceTests
{
    private readonly Mock<IBookRepository> _repository = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly IMapper _mapper;
    private readonly IValidator<BookRequest> _validator = new BookRequestValidator();

    public BookServiceTests()
    {
        var configuration = new MapperConfiguration(configuration =>
            configuration.AddProfile<LibraryMappingProfile>(), NullLoggerFactory.Instance);
        _mapper = configuration.CreateMapper();
    }

    private BookService CreateService() => new(_repository.Object, _unitOfWork.Object, _mapper, _validator);

    [Fact]
    public async Task GetByIdAsync_ReturnsBook_WhenExists()
    {
        _repository.Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Book { Id = 1, Title = "Clean Code", Author = "Robert Martin", Isbn = "123", TotalCopies = 2, AvailableCopies = 2 });

        var result = await CreateService().GetByIdAsync(1);

        result.Title.Should().Be("Clean Code");
    }

    [Fact]
    public async Task GetByIdAsync_ThrowsNotFoundException_WhenMissing()
    {
        _repository.Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync((Book?)null);

        var action = () => CreateService().GetByIdAsync(1);

        await action.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task CreateAsync_ThrowsConflict_WhenIsbnExists()
    {
        _repository.Setup(x => x.IsbnExistsAsync("123", null, It.IsAny<CancellationToken>())).ReturnsAsync(true);

        var action = () => CreateService().CreateAsync(new BookRequest("Title", "Author", "123", 1));

        await action.Should().ThrowAsync<ConflictException>();
    }

    [Fact]
    public async Task CreateAsync_SavesBook_WhenValid()
    {
        _repository.Setup(x => x.IsbnExistsAsync("123", null, It.IsAny<CancellationToken>())).ReturnsAsync(false);

        await CreateService().CreateAsync(new BookRequest("Title", "Author", "123", 1));

        _repository.Verify(x => x.AddAsync(It.Is<Book>(book => book.AvailableCopies == 1), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_ThrowsBusinessRule_WhenHasLoans()
    {
        _repository.Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Book { Id = 1, TotalCopies = 1, AvailableCopies = 0 });
        _repository.Setup(x => x.HasLoansAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(true);

        var action = () => CreateService().DeleteAsync(1);

        await action.Should().ThrowAsync<BusinessRuleException>();
    }
}
