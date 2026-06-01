using Microsoft.AspNetCore.Diagnostics;

namespace Products.API.ExceptionHandlers;

public class BadRequestExceptionHandler(ILogger<BadRequestExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext context, Exception exception, CancellationToken cancellationToken)
    {
        if (exception is not Microsoft.AspNetCore.Http.BadHttpRequestException ex) return false;

        var correlationId = context.Items["X-Correlation-Id"]?.ToString();
        if (correlationId != null)
            context.Response.Headers["x-correlation-id"] = correlationId;

        logger.LogWarning("Request inválido. ErrorCode: {ErrorCode}, Path: {Path}",
            "PRD-002", context.Request.Path);

        context.Response.StatusCode = 400;
        await context.Response.WriteAsJsonAsync(new
        {
            type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
            title = "Bad Request",
            status = 400,
            detail = "Los datos enviados son inválidos.",
            instance = context.Request.Path.Value,
            errorCode = "PRD-002",
            errorMessage = "Los datos del producto son inválidos."
        }, cancellationToken: cancellationToken);
        return true;
    }
}