using Microsoft.AspNetCore.Diagnostics;
using Notifications.API.Exceptions;

namespace Notifications.API.ExceptionHandlers;

public class BusinessRuleExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext context, Exception exception, CancellationToken cancellationToken)
    {
        if (exception is not BusinessRuleException ex) return false;

        var correlationId = context.Items["X-Correlation-Id"]?.ToString();
        if (correlationId != null)
            context.Response.Headers["x-correlation-id"] = correlationId;

        var (statusCode, type, title, detail) = (409,
        "https://tools.ietf.org/html/rfc7231#section-6.5.9",
        "Conflict",
        "No se puede procesar la solicitud.");

        context.Response.StatusCode = statusCode;
        await context.Response.WriteAsJsonAsync(new
        {
            type,
            title,
            status = statusCode,
            detail,
            instance = context.Request.Path.Value,
            errorCode = ex.ErrorCode,
            errorMessage = ex.Message,
            correlationId
        }, cancellationToken: cancellationToken);
        return true;
    }
}