using CostEstimating.Application.DTOs;
using CostEstimating.Application.Exceptions;
using CostEstimating.Domain.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace CostEstimating.Api.Middleware;

public sealed class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled request failure. Path={Path}", context.Request.Path);

            var status = ex switch
            {
                UnauthorizedException => StatusCodes.Status401Unauthorized,
                ForbiddenException => StatusCodes.Status403Forbidden,
                NotFoundException => StatusCodes.Status404NotFound,
                ConflictException => StatusCodes.Status409Conflict,
                InvalidStateTransitionException => StatusCodes.Status409Conflict,
                DomainException => StatusCodes.Status400BadRequest,
                ArgumentException => StatusCodes.Status400BadRequest,
                _ => StatusCodes.Status500InternalServerError
            };

            var response = new ErrorResponse(
                $"https://httpstatuses.com/{status}",
                ex switch
                {
                    UnauthorizedException => "Unauthorized",
                    ForbiddenException => "Forbidden",
                    NotFoundException => "Not Found",
                    ConflictException or InvalidStateTransitionException => "Conflict",
                    DomainException or ArgumentException => "Validation failed",
                    _ => "Unexpected error"
                },
                status,
                status == 500 ? "An unexpected error occurred." : ex.Message,
                context.TraceIdentifier);

            context.Response.StatusCode = status;
            context.Response.ContentType = "application/problem+json";
            await context.Response.WriteAsJsonAsync(response);
        }
    }
}
