using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace LibraryManagement.Application.Common.Behaviors;

public static class ValidationBehavior
{
    public static async Task ValidateAsync<TRequest>(
        TRequest request,
        IValidator<TRequest> validator,
        CancellationToken cancellationToken = default)
    {
        var result = await validator.ValidateAsync(request, cancellationToken);
        if (!result.IsValid)
        {
            throw new ValidationException(result.Errors);
        }
    }
}

public static class ValidationBehaviorExtensions
{
    public static IServiceCollection AddValidationBehaviors(this IServiceCollection services)
    {
        return services;
    }
}
