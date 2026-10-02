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
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace LibraryManagement.Tests;

public class MemberServiceTests
{
    private readonly Mock<IMemberRepository> _repository = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly IMapper _mapper;
    private readonly IValidator<MemberRequest> _validator = new MemberRequestValidator();

    public MemberServiceTests()
    {
        var configuration = new MapperConfiguration(configuration =>
            configuration.AddProfile<LibraryMappingProfile>(), NullLoggerFactory.Instance);
        _mapper = configuration.CreateMapper();
    }

    private MemberService CreateService() => new(_repository.Object, _unitOfWork.Object, _mapper, _validator);

    [Fact]
    public async Task GetByIdAsync_ReturnsMember_WhenExists()
    {
        _repository.Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Member { Id = 1, FullName = "Ada Lovelace", Email = "ada@example.com" });

        var result = await CreateService().GetByIdAsync(1);

        result.FullName.Should().Be("Ada Lovelace");
    }

    [Fact]
    public async Task GetByIdAsync_ThrowsNotFoundException_WhenMissing()
    {
        _repository.Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync((Member?)null);

        var action = () => CreateService().GetByIdAsync(1);

        await action.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task CreateAsync_ThrowsConflict_WhenEmailExists()
    {
        _repository.Setup(x => x.EmailExistsAsync("ada@example.com", null, It.IsAny<CancellationToken>())).ReturnsAsync(true);

        var action = () => CreateService().CreateAsync(new MemberRequest("Ada Lovelace", "ada@example.com", null));

        await action.Should().ThrowAsync<ConflictException>();
    }

    [Fact]
    public async Task CreateAsync_SavesMember_WhenValid()
    {
        _repository.Setup(x => x.EmailExistsAsync("ada@example.com", null, It.IsAny<CancellationToken>())).ReturnsAsync(false);

        await CreateService().CreateAsync(new MemberRequest("Ada Lovelace", "ada@example.com", null));

        _repository.Verify(x => x.AddAsync(It.Is<Member>(member => member.FullName == "Ada Lovelace"), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_ThrowsBusinessRule_WhenMemberHasLoans()
    {
        _repository.Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Member { Id = 1, FullName = "Ada Lovelace", Email = "ada@example.com" });
        _repository.Setup(x => x.HasLoansAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(true);

        var action = () => CreateService().DeleteAsync(1);

        await action.Should().ThrowAsync<BusinessRuleException>();
    }
}
