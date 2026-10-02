using FluentValidation;
using LibraryManagement.Domain.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace LibraryManagement.Api.Middleware;

public class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var level = exception switch
        {
            NotFoundException => LogLevel.Information,
            ConflictException or BusinessRuleException or ValidationException => LogLevel.Warning,
            _ => LogLevel.Error
        };
        _logger.Log(level, exception, "An exception occurred while processing the request.");

        httpContext.Response.StatusCode = exception switch
        {
            JsonException => 400,
            NotFoundException => 404,
            ConflictException => 409,
            BusinessRuleException => 400,
            ValidationException => 422,
            _ => 500
        };

        var problemDetails = new ProblemDetails
        {
            Title = httpContext.Response.StatusCode switch
            {
                404 => "Not Found",
                409 => "Conflict",
                422 => "Validation Error",
                500 => "Internal Server Error",
                _ => "Error"
            },
            Detail = exception.Message,
            Status = httpContext.Response.StatusCode,
            Instance = httpContext.Request.Path
        };

        problemDetails.Extensions["TraceId"] = httpContext.TraceIdentifier;

        if (exception is ValidationException validationEx)
        {
            problemDetails.Extensions["ValidationErrors"] = validationEx.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());
        }

        httpContext.Response.ContentType = "application/problem+json";
        await httpContext.Response.WriteAsJsonAsync(problemDetails, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        }, cancellationToken);
        return true;
    }
}
