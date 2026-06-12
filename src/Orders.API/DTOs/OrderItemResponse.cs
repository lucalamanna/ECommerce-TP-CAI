namespace Orders.API.DTOs;

/// <summary>Producto incluido en una orden.</summary>
public class OrderItemResponse
{
    /// <summary>Identificador del producto.</summary>
    /// <example>3fa85f64-5717-4562-b3fc-2c963f66afa6</example>
    public Guid ProductoId { get; set; }

    /// <summary>Cantidad del producto en la orden.</summary>
    /// <example>2</example>
    public int Cantidad { get; set; }

    /// <summary>Precio unitario capturado al crear la orden.</summary>
    /// <example>1500.00</example>
    public decimal PrecioUnitario { get; set; }
}
