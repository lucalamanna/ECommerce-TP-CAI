using Microsoft.AspNetCore.Diagnostics;
using System.Text.RegularExpressions;

namespace Products.API.ExceptionHandlers;

public class BadRequestExceptionHandler(ILogger<BadRequestExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext context, Exception exception, CancellationToken cancellationToken)
    {
        if (exception is not Microsoft.AspNetCore.Http.BadHttpRequestException ex) return false;

        var correlationId = context.Items["X-Correlation-Id"]?.ToString();
        if (correlationId != null)
            context.Response.Headers["x-correlation-id"] = correlationId;

        var errorMessage = ExtraerMensaje(ex);

        logger.LogWarning("Request inválido. ErrorCode: {ErrorCode}, Message: {Message}, Path: {Path}",
            "PRD-002", errorMessage, context.Request.Path);

        context.Response.StatusCode = 400;
        await context.Response.WriteAsJsonAsync(new
        {
            type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
            title = "Bad Request",
            status = 400,
            detail = "Los datos enviados son inválidos.",
            instance = context.Request.Path.Value,
            errorCode = "PRD-002",
            errorMessage
        }, cancellationToken: cancellationToken);
        return true;
    }

    private static string ExtraerMensaje(Microsoft.AspNetCore.Http.BadHttpRequestException ex)
    {
        var inner = ex.InnerException?.Message ?? ex.Message;

        // Extraer el path del campo: "Path: $.precio" → "precio"
        var pathMatch = Regex.Match(inner, @"Path:\s*\$\.(\w+)");
        if (pathMatch.Success)
        {
            var campo = CapitalizarPrimera(pathMatch.Groups[1].Value);
            return $"El campo '{campo}' tiene un formato inválido";
        }

        // JSON completamente malformado (sin path de campo)
        if (inner.Contains("invalid start of a value") ||
         inner.Contains("JsonReaderException") ||
         inner.Contains("trailing comma") ||
         inner.Contains("is invalid after a value"))
            return "El cuerpo de la solicitud contiene JSON inválido";

        return "Los datos del producto son inválidos";
    }

    private static string CapitalizarPrimera(string s) =>
        string.IsNullOrEmpty(s) ? s : char.ToUpper(s[0]) + s[1..];
}