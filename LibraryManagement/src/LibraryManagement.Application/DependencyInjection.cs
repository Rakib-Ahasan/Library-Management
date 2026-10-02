using AutoMapper;
using FluentValidation;
using LibraryManagement.Application.Common.Mappings;
using LibraryManagement.Application.Interfaces.Services;
using LibraryManagement.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace LibraryManagement.Application;

/// <summary>
/// Extension methods for configuring application services.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Adds application services to the dependency injection container.
    /// </summary>
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // Register service implementations
        services.AddScoped<IBookService, BookService>();
        services.AddScoped<IMemberService, MemberService>();
        services.AddScoped<ILoanService, LoanService>();

        // Register AutoMapper with profiles from the assembly
        services.AddAutoMapper(configuration =>
            configuration.AddMaps(typeof(LibraryMappingProfile).Assembly));

        // Register FluentValidation validators from the application assembly
        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);

        return services;
    }
}
