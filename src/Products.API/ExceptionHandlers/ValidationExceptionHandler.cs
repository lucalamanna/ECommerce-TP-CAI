using Microsoft.AspNetCore.Diagnostics;
using Products.API.Exceptions;

namespace Products.API.ExceptionHandlers;
public class ValidationExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
    HttpContext context, Exception exception, CancellationToken cancellationToken)
    {
        if (exception is not ValidationException ex) return false;

        var correlationId = context.Items["X-Correlation-Id"]?.ToString();
        if (correlationId != null)
            context.Response.Headers["x-correlation-id"] = correlationId;

        context.Response.StatusCode = 400;
        await context.Response.WriteAsJsonAsync(new
        {
            type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
            title = "Bad Request",
            status = 400,
            detail = "Los datos enviados son inválidos.",
            instance = context.Request.Path.Value,
            errorCode = ex.ErrorCode,
            errorMessage = ex.Message,
            correlationId
        }, cancellationToken: cancellationToken);
        return true;
    }
}