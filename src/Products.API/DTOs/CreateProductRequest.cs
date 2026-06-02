using System.ComponentModel.DataAnnotations;

namespace Products.API.DTOs;

/// <summary>
/// Datos necesarios para crear un nuevo producto.
/// </summary>
public class CreateProductRequest
{
    /// <summary>Nombre del producto. Requerido, máximo 100 caracteres.</summary>
    /// <example>Notebook Dell XPS 15</example>
    [MaxLength(100)] public string Nombre { get; set; } = string.Empty;

    /// <summary>Descripción del producto (opcional). Máximo 500 caracteres.</summary>
    /// <example>Laptop 15 pulgadas, 32GB RAM</example>
    [MaxLength(500)] public string? Descripcion { get; set; }

    /// <summary>Precio del producto. Debe ser mayor a 0.</summary>
    /// <example>1500.00</example>
    public decimal Precio { get; set; }

    /// <summary>Stock disponible. Debe ser mayor o igual a 0.</summary>
    /// <example>10</example>
    public int? Stock { get; set; }

    /// <summary>Categoría del producto.</summary>
    /// <example>Electrónica</example>
    public string Categoria { get; set; } = string.Empty;
}