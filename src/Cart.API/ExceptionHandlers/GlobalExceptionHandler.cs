using Microsoft.AspNetCore.Diagnostics;

namespace Cart.API.ExceptionHandlers;

public class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext context, Exception exception, CancellationToken cancellationToken)
    {
        logger.LogError(exception, "Error inesperado en {Path}", context.Request.Path);

        var correlationId = context.Items["X-Correlation-Id"]?.ToString();
        if (correlationId != null)
            context.Response.Headers["x-correlation-id"] = correlationId;

        context.Response.StatusCode = 500;
        await context.Response.WriteAsJsonAsync(new
        {
            type = "https://tools.ietf.org/html/rfc7231#section-6.6.1",
            title = "Internal Server Error",
            status = 500,
            detail = "Ocurrió un error interno.",
            instance = context.Request.Path.Value,
            errorCode = "CRT-005",
            errorMessage = "Error interno al procesar el carrito.",
            correlationId
        }, cancellationToken: cancellationToken);
        return true;
    }
}