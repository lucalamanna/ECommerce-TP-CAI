namespace Users.API.DTOs;

/// <summary>Estructura estándar de respuesta de error.</summary>
public class ErrorResponse
{
    /// <summary>URI de referencia del tipo de error.</summary>
    /// <example>https://tools.ietf.org/html/rfc7231#section-6.5.4</example>
    public string Type { get; set; } = string.Empty;

    /// <summary>Título del error.</summary>
    /// <example>Not Found</example>
    public string Title { get; set; } = string.Empty;

    /// <summary>Código HTTP del error.</summary>
    /// <example>404</example>
    public int Status { get; set; }

    /// <summary>Detalle del error.</summary>
    /// <example>El recurso solicitado no fue encontrado.</example>
    public string Detail { get; set; } = string.Empty;

    /// <summary>Path del endpoint que generó el error.</summary>
    /// <example>/api/users/register</example>
    public string Instance { get; set; } = string.Empty;

    /// <summary>Código de error del catálogo.</summary>
    /// <example>USR-001</example>
    public string ErrorCode { get; set; } = string.Empty;

    /// <summary>Mensaje descriptivo del error.</summary>
    /// <example>El email ya está registrado.</example>
    public string ErrorMessage { get; set; } = string.Empty;
}