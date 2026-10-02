using AutoMapper;
using LibraryManagement.Application.Dtos;
using LibraryManagement.Domain.Entities;

namespace LibraryManagement.Application.Common.Mappings;

public class LibraryMappingProfile : Profile
{
    public LibraryMappingProfile()
    {
        CreateMap<Book, BookResponse>().ReverseMap()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.Loans, opt => opt.Ignore());

        CreateMap<BookRequest, Book>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.AvailableCopies, opt => opt.MapFrom(src => src.TotalCopies))
            .ForMember(dest => dest.Loans, opt => opt.Ignore());

        CreateMap<Member, MemberResponse>().ReverseMap()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.Loans, opt => opt.Ignore());

        CreateMap<MemberRequest, Member>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.JoinedOn, opt => opt.Ignore())
            .ForMember(dest => dest.Loans, opt => opt.Ignore());

        CreateMap<Loan, LoanResponse>()
            .ForCtorParam(nameof(LoanResponse.Id), opt => opt.MapFrom(src => src.Id))
            .ForCtorParam(nameof(LoanResponse.BookId), opt => opt.MapFrom(src => src.BookId))
            .ForCtorParam(nameof(LoanResponse.BookTitle), opt => opt.MapFrom(src => src.Book.Title))
            .ForCtorParam(nameof(LoanResponse.MemberId), opt => opt.MapFrom(src => src.MemberId))
            .ForCtorParam(nameof(LoanResponse.MemberName), opt => opt.MapFrom(src => src.Member.FullName))
            .ForCtorParam(nameof(LoanResponse.BorrowedOn), opt => opt.MapFrom(src => src.BorrowedOn))
            .ForCtorParam(nameof(LoanResponse.DueDate), opt => opt.MapFrom(src => src.DueDate))
            .ForCtorParam(nameof(LoanResponse.ReturnedOn), opt => opt.MapFrom(src => src.ReturnedOn))
            .ForCtorParam(nameof(LoanResponse.IsOverdue), opt => opt.MapFrom(src => src.IsOverdue(DateTime.UtcNow)));

        CreateMap<BorrowRequest, Loan>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.BookId, opt => opt.MapFrom(src => src.BookId))
            .ForMember(dest => dest.MemberId, opt => opt.MapFrom(src => src.MemberId))
            .ForMember(dest => dest.BorrowedOn, opt => opt.Ignore())
            .ForMember(dest => dest.DueDate, opt => opt.MapFrom(src => DateTime.UtcNow.AddDays(src.Days)))
            .ForMember(dest => dest.ReturnedOn, opt => opt.Ignore())
            .ForMember(dest => dest.Book, opt => opt.Ignore())
            .ForMember(dest => dest.Member, opt => opt.Ignore());
    }
}
