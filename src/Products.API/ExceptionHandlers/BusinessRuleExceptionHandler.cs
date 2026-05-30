using Microsoft.AspNetCore.Diagnostics;
using Products.API.Exceptions;

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
            errorMessage = ex.Message
        }, cancellationToken: cancellationToken);
        return true;
    }
}