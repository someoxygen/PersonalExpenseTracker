using ExpenseTracker.Application.Common;
using ExpenseTracker.Domain.Common;
using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
namespace ExpenseTracker.Api.Middleware;

public sealed class ApiExceptionHandler(IProblemDetailsService problems, ILogger<ApiExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext context, Exception exception, CancellationToken ct)
    {
        var status = exception switch { AppException e => e.Status, ValidationException or DomainException => 400, _ => 500 };
        context.Response.StatusCode = status;
        if (status == 500) logger.LogError("Unhandled API error of type {ExceptionType}. Trace {TraceId}", exception.GetType().Name, context.TraceIdentifier);
        ProblemDetails detail = exception is ValidationException validation
            ? new ValidationProblemDetails(validation.Errors.GroupBy(e => e.PropertyName)
                .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).Distinct().ToArray()))
            : new ProblemDetails();
        detail.Status = status;
        detail.Title = status == 500 ? "An unexpected error occurred." : exception.Message;
        detail.Extensions["traceId"] = context.TraceIdentifier;
        return await problems.TryWriteAsync(new ProblemDetailsContext { HttpContext = context, ProblemDetails = detail });
    }
}
