using FluentAssertions;
using LibraryManagement.Application.Dtos;
using LibraryManagement.Application.Validators;

namespace LibraryManagement.Tests;

public class ValidatorTests
{
    [Fact]
    public async Task BookRequestValidator_InvalidWhenTitleEmpty()
    {
        var result = await new BookRequestValidator().ValidateAsync(new BookRequest(string.Empty, "Author", "123", 1));

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(error => error.PropertyName == nameof(BookRequest.Title));
    }

    [Fact]
    public async Task BorrowRequestValidator_InvalidWhenDaysOutOfRange()
    {
        var result = await new BorrowRequestValidator().ValidateAsync(new BorrowRequest(1, 1, 61));

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(error => error.PropertyName == nameof(BorrowRequest.Days));
    }
}
