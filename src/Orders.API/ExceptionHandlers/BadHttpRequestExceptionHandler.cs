using Microsoft.AspNetCore.Diagnostics;
using System.Text.RegularExpressions;

namespace Orders.API.ExceptionHandlers;

public class BadHttpRequestExceptionHandler(ILogger<BadHttpRequestExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext context, Exception exception, CancellationToken cancellationToken)
    {
        if (exception is not BadHttpRequestException ex) return false;

        var correlationId = context.Items["X-Correlation-Id"]?.ToString();
        if (correlationId != null)
            context.Response.Headers["x-correlation-id"] = correlationId;

        var errorMessage = ObtenerMensajeDetallado(ex);

        logger.LogWarning("Request inválido. {ErrorCode}, Campo: {ErrorMessage}, Path: {Path}",
            "ORD-002", errorMessage, context.Request.Path);

        context.Response.StatusCode = 400;
        await context.Response.WriteAsJsonAsync(new
        {
            type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
            title = "Bad Request",
            status = 400,
            detail = "Los datos enviados son inválidos.",
            instance = context.Request.Path.Value,
            errorCode = "ORD-002",
            errorMessage,
            correlationId
        }, cancellationToken: cancellationToken);

        return true;

    }


    private static string ObtenerMensajeDetallado(BadHttpRequestException ex)
    {
        var inner = ex.InnerException?.Message ?? ex.Message;

        var pathMatch = Regex.Match(inner, @"Path:\s*\$\.(\w+)");
        if (pathMatch.Success)
        {
            var campo = CapitalizarPrimera(pathMatch.Groups[1].Value);
            return $"El campo '{campo}' tiene un formato inválido.";
        }

        if (ex.InnerException is System.Text.Json.JsonException)
            return "El cuerpo de la solicitud contiene JSON inválido.";

        return "Los datos de la orden son inválidos.";
    }

    private static string CapitalizarPrimera(string s) =>
        string.IsNullOrEmpty(s) ? s : char.ToUpper(s[0]) + s[1..];
}
