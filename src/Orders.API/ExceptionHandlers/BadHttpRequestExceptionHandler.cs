using Microsoft.AspNetCore.Diagnostics;

namespace Orders.API.ExceptionHandlers
{
    public class BadHttpRequestExceptionHandler(ILogger<BadHttpRequestExceptionHandler> logger) : IExceptionHandler
    {
        public async ValueTask<bool> TryHandleAsync(
            HttpContext context, Exception exception, CancellationToken cancellationToken)
        {
            if (exception is not BadHttpRequestException) return false;

            var correlationId = context.Items["X-Correlation-Id"]?.ToString();
            if (correlationId != null)
                context.Response.Headers["x-correlation-id"] = correlationId;

            logger.LogWarning(exception, "Request inválido. {ErrorCode}", "ORD-002");

            context.Response.StatusCode = 400;
            await context.Response.WriteAsJsonAsync(new
            {
                type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
                title = "Bad Request",
                status = 400,
                detail = "Los datos enviados son inválidos.",
                instance = context.Request.Path.Value,
                errorCode = "ORD-002",
                errorMessage = "Los datos de la orden son inválidos.",
                correlationId
            }, cancellationToken: cancellationToken);

            return true;
        }
    }
}
