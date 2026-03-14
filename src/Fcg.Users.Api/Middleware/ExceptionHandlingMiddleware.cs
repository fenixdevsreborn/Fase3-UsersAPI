using System.Net;
using System.Text.Json;
using Fcg.Users.Api.Observability;
using Fcg.Users.Application.Exceptions;
using Microsoft.AspNetCore.Http;

namespace Fcg.Users.Api.Middleware;

/// <summary>Maps domain exceptions to HTTP responses and returns JSON. Logs with structured properties (TraceId, CorrelationId).</summary>
public class ExceptionHandlingMiddleware
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
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        // Use generic messages in response to avoid leaking sensitive or internal details (full message is still logged).
        var (statusCode, message) = exception switch
        {
            UnauthorizedException => (HttpStatusCode.Unauthorized, "Unauthorized"),
            NotFoundException => (HttpStatusCode.NotFound, "Resource not found"),
            ConflictException => (HttpStatusCode.Conflict, "Conflict"),
            ArgumentException => (HttpStatusCode.BadRequest, "Bad request"),
            _ => (HttpStatusCode.InternalServerError, "An error occurred.")
        };

        var traceId = ObservabilityContext.GetCurrentTraceId();
        var correlationId = ObservabilityContext.GetCurrentCorrelationId();

        if ((int)statusCode >= 500)
            _logger.LogError(exception, "Unhandled error. {TraceId} {CorrelationId} {Message}", traceId, correlationId, exception.Message);
        else
            _logger.LogWarning(exception, "Application error. {TraceId} {CorrelationId} {Message}", traceId, correlationId, exception.Message);

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;

        var body = new { message };
        await context.Response.WriteAsync(JsonSerializer.Serialize(body));
    }
}
