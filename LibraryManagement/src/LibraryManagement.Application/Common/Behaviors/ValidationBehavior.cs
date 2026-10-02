using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace LibraryManagement.Application.Common.Behaviors;

/// <summary>
/// Pipeline behavior for request validation using FluentValidation.
/// This runs before the actual handler to validate the request.
/// Note: This is a MediatR-style behavior, but we register it as a workaround for validation.
/// </summary>
/// <typeparam name="TRequest">The request type</typeparam>
/// <typeparam name="TResponse">The response type</typeparam>
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

/// <summary>
/// Extension method to add validation behavior to the service collection.
/// </summary>
public static class ValidationBehaviorExtensions
{
    public static IServiceCollection AddValidationBehaviors(this IServiceCollection services)
    {
        return services;
    }
}
