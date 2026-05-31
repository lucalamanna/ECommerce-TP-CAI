namespace Cart.API.DTOs;

/// <summary>Estructura estándar de respuesta de error.</summary>
public class ErrorResponse
{
    /// <summary>URI de referencia del tipo de error.</summary>
    public string Type { get; set; } = string.Empty;

    /// <summary>Título del error.</summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>Código HTTP del error.</summary>
    public int Status { get; set; }

    /// <summary>Detalle del error.</summary>
    public string Detail { get; set; } = string.Empty;

    /// <summary>Path del endpoint que generó el error.</summary>
    public string Instance { get; set; } = string.Empty;

    /// <summary>Código de error del catálogo.</summary>
    public string ErrorCode { get; set; } = string.Empty;

    /// <summary>Mensaje descriptivo del error.</summary>
    public string ErrorMessage { get; set; } = string.Empty;
}