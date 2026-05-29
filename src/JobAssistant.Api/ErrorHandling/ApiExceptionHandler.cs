using JobAssistant.Application.Common.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace JobAssistant.Api.ErrorHandling;

public sealed class ApiExceptionHandler(ILogger<ApiExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        var problem = CreateProblemDetails(httpContext, exception);

        if (problem.Status >= StatusCodes.Status500InternalServerError)
        {
            logger.LogError(exception, "Unhandled exception for {Path}", httpContext.Request.Path);
        }
        else
        {
            logger.LogWarning(exception, "Request failure for {Path}: {Status}", httpContext.Request.Path, problem.Status);
        }

        httpContext.Response.StatusCode = problem.Status!.Value;

        await httpContext.Response.WriteAsJsonAsync(problem, cancellationToken);

        return true;
    }

    private static ProblemDetails CreateProblemDetails(HttpContext context, Exception exception)
    {
        var path = context.Request.Path.Value ?? string.Empty;

        return exception switch
        {
            ValidationException validation => new ProblemDetails
            {
                Type = "https://jobassistant/errors/validation",
                Title = "Validation failed",
                Status = StatusCodes.Status400BadRequest,
                Detail = validation.Message,
                Instance = path,
                Extensions = { ["errors"] = validation.Errors }
            },
            NotFoundException notFound => new ProblemDetails
            {
                Type = "https://jobassistant/errors/not-found",
                Title = "Resource not found",
                Status = StatusCodes.Status404NotFound,
                Detail = notFound.Message,
                Instance = path
            },
            ConflictException conflict => new ProblemDetails
            {
                Type = "https://jobassistant/errors/conflict",
                Title = "Conflict",
                Status = StatusCodes.Status409Conflict,
                Detail = conflict.Message,
                Instance = path
            },
            ExternalServiceException externalService => new ProblemDetails
            {
                Type = "https://jobassistant/errors/external-service",
                Title = "External service error",
                Status = StatusCodes.Status502BadGateway,
                Detail = externalService.Message,
                Instance = path
            },
            _ => new ProblemDetails
            {
                Type = "https://jobassistant/errors/unexpected",
                Title = "Unexpected server error",
                Status = StatusCodes.Status500InternalServerError,
                Detail = "An unexpected error occurred.",
                Instance = path
            }
        };
    }
}
