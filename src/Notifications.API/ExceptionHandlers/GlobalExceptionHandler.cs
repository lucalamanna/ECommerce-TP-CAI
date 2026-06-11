using Microsoft.AspNetCore.Diagnostics;

namespace Notifications.API.ExceptionHandlers;

public class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext context, Exception exception, CancellationToken cancellationToken)
    {
        logger.LogError(exception, "Error inesperado. Path: {Path}", context.Request.Path);

        var correlationId = context.Items["X-Correlation-Id"]?.ToString();
        if (correlationId != null)
            context.Response.Headers["x-correlation-id"] = correlationId;

        context.Response.StatusCode = 500;
        await context.Response.WriteAsJsonAsync(new
        {
            type = "https://tools.ietf.org/html/rfc7231#section-6.6.1",
            title = "Internal Server Error",
            status = 500,
            detail = "Ocurrio un error interno.",
            instance = context.Request.Path.Value,
            errorCode = "NTF-004",
            errorMessage = "Error interno al procesar la notificacion."
        }, cancellationToken);
        return true;
    }
}