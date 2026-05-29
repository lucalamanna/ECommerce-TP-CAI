using System.ComponentModel.DataAnnotations;

namespace Products.API.DTOs;

/// <summary>
/// Datos para actualizar un producto existente.
/// </summary>
public class UpdateProductRequest
{
    /// <summary>Nombre del producto. Requerido, máximo 100 caracteres.</summary>
    /// <example>Notebook Dell XPS 15</example>
    [Required][MaxLength(100)] public string Nombre { get; set; } = string.Empty;

    /// <summary>Descripción del producto (opcional). Máximo 500 caracteres.</summary>
    /// <example>Laptop 15 pulgadas, 64GB RAM</example>
    [MaxLength(500)] public string? Descripcion { get; set; }

    /// <summary>Precio del producto. Debe ser mayor a 0.</summary>
    /// <example>1750.00</example>
    [Required][Range(0.01, double.MaxValue)] public decimal Precio { get; set; }

    /// <summary>Stock disponible. Debe ser mayor o igual a 0.</summary>
    /// <example>8</example>
    [Required][Range(0, int.MaxValue)] public int Stock { get; set; }

    /// <summary>Categoría del producto.</summary>
    /// <example>Electrónica</example>
    [Required] public string Categoria { get; set; } = string.Empty;
}