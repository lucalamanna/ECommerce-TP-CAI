namespace Orders.API.Models
{
    /// <summary>Entidad de dominio que representa un ítem dentro de una orden.</summary>
    public class OrderItem
    {
        /// <summary>Identificador del producto referenciado.</summary>
        public Guid ProductoId { get; set; }

        /// <summary>Cantidad solicitada del producto.</summary>
        public int Cantidad { get; set; }

        /// <summary>Precio unitario del producto al momento de crear la orden.</summary>
        public decimal PrecioUnitario { get; set; }
    }
}
