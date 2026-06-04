using Microsoft.AspNetCore.Diagnostics;
using Users.API.Exceptions;

namespace Users.API.ExceptionHandlers;

public class BusinessRuleExceptionHandler(ILogger<BusinessRuleExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext context, Exception exception, CancellationToken cancellationToken)
    {
        if (exception is not BusinessRuleException ex) return false;

        var correlationId = context.Items["X-Correlation-Id"]?.ToString();
        if (correlationId != null)
            context.Response.Headers["x-correlation-id"] = correlationId;

        logger.LogWarning("Regla de negocio violada. ErrorCode: {ErrorCode}, Message: {Message}, Path: {Path}",
            ex.ErrorCode, ex.Message, context.Request.Path);

        var (status, type, title, detail) = ex.ErrorCode switch
        {
            "USR-001" => (409, "https://tools.ietf.org/html/rfc7231#section-6.5.9", "Conflict", "Ya existe un recurso con esos datos."),
            "USR-003" => (401, "https://tools.ietf.org/html/rfc7235#section-3.1", "Unauthorized", "Las credenciales no son válidas."),
            "USR-004" => (403, "https://tools.ietf.org/html/rfc7231#section-6.5.3", "Forbidden", "El acceso está prohibido."),
            "USR-005" => (403, "https://tools.ietf.org/html/rfc7231#section-6.5.3", "Forbidden", "El acceso está prohibido."),
            _ => (409, "https://tools.ietf.org/html/rfc7231#section-6.5.9", "Conflict", "No se puede procesar la solicitud.")
        };

        context.Response.StatusCode = status;
        await context.Response.WriteAsJsonAsync(new
        {
            type,
            title,
            status,
            detail,
            instance = context.Request.Path.Value,
            errorCode = ex.ErrorCode,
            errorMessage = ex.Message
        }, cancellationToken: cancellationToken);
        return true;
    }
}