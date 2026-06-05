using Microsoft.AspNetCore.Diagnostics;
using Products.API.Exceptions;

namespace Products.API.ExceptionHandlers;

public class BusinessRuleExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext context, Exception exception, CancellationToken cancellationToken)
    {
        if (exception is not BusinessRuleException ex) return false;

        var correlationId = context.Items["X-Correlation-Id"]?.ToString();
        if (correlationId != null)
            context.Response.Headers["x-correlation-id"] = correlationId;

        var (statusCode, type, title, detail) = ex.ErrorCode switch
        {
            "PRD-003" => (409, "https://tools.ietf.org/html/rfc7231#section-6.5.9", "Conflict", "Ya existe un recurso con esos datos."),
            "PRD-004" => (409, "https://tools.ietf.org/html/rfc7231#section-6.5.9", "Conflict", "No se puede eliminar el recurso."),
            _ => (409, "https://tools.ietf.org/html/rfc7231#section-6.5.9", "Conflict", "No se puede procesar la solicitud.")
        };
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