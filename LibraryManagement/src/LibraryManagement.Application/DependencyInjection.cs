using AutoMapper;
using FluentValidation;
using LibraryManagement.Application.Common.Mappings;
using LibraryManagement.Application.Interfaces.Services;
using LibraryManagement.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace LibraryManagement.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IBookService, BookService>();
        services.AddScoped<IMemberService, MemberService>();
        services.AddScoped<ILoanService, LoanService>();

        services.AddAutoMapper(configuration =>
            configuration.AddMaps(typeof(LibraryMappingProfile).Assembly));

        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);

        return services;
    }
}
