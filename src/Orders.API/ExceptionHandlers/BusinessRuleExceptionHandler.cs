using Microsoft.AspNetCore.Diagnostics;
using Orders.API.Exceptions;

namespace Orders.API.ExceptionHandlers
{
    public class BusinessRuleExceptionHandler : IExceptionHandler
    {
        public async ValueTask<bool> TryHandleAsync(
       HttpContext context, Exception exception, CancellationToken cancellationToken)
        {
            if (exception is not BusinessRuleException ex) return false;

            var (status, type, title, detail) = ex.ErrorCode switch
            {
                "ORD-005" => (422, "https://tools.ietf.org/html/rfc4918#section-11.2", "Unprocessable Entity", "No se puede procesar la solicitud."),
                "ORD-006" => (409, "https://tools.ietf.org/html/rfc7231#section-6.5.9", "Conflict", "No se puede modificar el estado."),
                "ORD-008" => (409, "https://tools.ietf.org/html/rfc7231#section-6.5.9", "Conflict", "No se puede eliminar la orden."),
                _ => (409, "https://tools.ietf.org/html/rfc7231#section-6.5.9", "Conflict", "No se puede procesar la solicitud.")
            };
            
            var correlationId = context.Items["X-Correlation-Id"]?.ToString();
            if (correlationId != null)                                          
                context.Response.Headers["x-correlation-id"] = correlationId;

            context.Response.StatusCode = status;
            await context.Response.WriteAsJsonAsync(new
            {
                type,
                title,
                status,
                detail,
                instance = context.Request.Path.Value,
                errorCode = ex.ErrorCode,
                errorMessage = ex.Message,
                correlationId
            }, cancellationToken: cancellationToken);

            return true;
        }
    }
}   
