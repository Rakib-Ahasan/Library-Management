using FluentValidation;
using LibraryManagement.Application.Dtos;
using System;
using System.Collections.Generic;
using System.Text;

namespace LibraryManagement.Application.Validators
{
    public class BookRequestValidator : AbstractValidator<BookRequest>
    {
        public BookRequestValidator()
        {
            RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
            RuleFor(x => x.Author).NotEmpty().MaximumLength(150);
            RuleFor(x => x.Isbn).NotEmpty().MaximumLength(20);
            RuleFor(x => x.TotalCopies).InclusiveBetween(1, 1000);
        }
    }

    public class MemberRequestValidator : AbstractValidator<MemberRequest>
    {
        public MemberRequestValidator()
        {
            RuleFor(x => x.FullName).NotEmpty().MaximumLength(150);
            RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(150);
            RuleFor(x => x.Phone).MaximumLength(20);
        }
    }

    public class BorrowRequestValidator : AbstractValidator<BorrowRequest>
    {
        public BorrowRequestValidator()
        {
            RuleFor(x => x.BookId).GreaterThan(0);
            RuleFor(x => x.MemberId).GreaterThan(0);
            RuleFor(x => x.Days).InclusiveBetween(1, 60);
        }
    }
}
