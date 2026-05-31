namespace Cart.API.DTOs;

/// <summary>Datos para actualizar la cantidad de un item del carrito.</summary>
public class UpdateItemRequest
{
    /// <summary>Nueva cantidad del producto.</summary>
    /// <example>4</example>
    public int Cantidad { get; set; }
}