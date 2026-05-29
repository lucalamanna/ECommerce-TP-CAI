namespace Products.API.DTOs;

/// <summary>Respuesta de error estándar de la API.</summary>
public class ErrorResponse
{
    /// <summary>URI que identifica el tipo de error.</summary>
    /// <example>https://tools.ietf.org/html/rfc7231#section-6.5.4</example>
    public string Type { get; set; } = string.Empty;

    /// <summary>Título del error.</summary>
    /// <example>Not Found</example>
    public string Title { get; set; } = string.Empty;

    /// <summary>Código HTTP del error.</summary>
    /// <example>404</example>
    public int Status { get; set; }

    /// <summary>Descripción detallada del error.</summary>
    /// <example>El recurso solicitado no fue encontrado.</example>
    public string Detail { get; set; } = string.Empty;

    /// <summary>Path del endpoint que generó el error.</summary>
    /// <example>/api/products/3fa85f64-5717-4562-b3fc-2c963f66afa6</example>
    public string Instance { get; set; } = string.Empty;

    /// <summary>Código de error del catálogo de Products API.</summary>
    /// <example>PRD-001</example>
    public string ErrorCode { get; set; } = string.Empty;

    /// <summary>Mensaje descriptivo del error.</summary>
    /// <example>Producto no encontrado.</example>
    public string ErrorMessage { get; set; } = string.Empty;
}