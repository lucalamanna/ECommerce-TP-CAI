namespace Cart.API.Models;

/// <summary>Representa el carrito de compras de un usuario.</summary>
public class Cart
{
    /// <summary>Identificador del usuario dueño del carrito.</summary>
    /// <example>a1b2c3d4-0000-0000-0000-111122223333</example>
    public Guid UsuarioId { get; set; }

    /// <summary>Lista de productos en el carrito.</summary>
    public List<CartItem> Items { get; set; } = [];

    /// <summary>Fecha de última actualización del carrito.</summary>
    /// <example>2024-03-10T10:45:00Z</example>
    public DateTime FechaActualizacion { get; set; }
}