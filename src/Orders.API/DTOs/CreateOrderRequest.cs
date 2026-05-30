namespace Orders.API.DTOs
{
    /// <summary>Datos para crear una nueva orden.</summary>
    public class CreateOrderRequest
    {
        /// <summary>Identificador del usuario que crea la orden.</summary>
        /// <example>a1b2c3d4-0000-0000-0000-111122223333</example>
        public Guid UsuarioId { get; set; }

        /// <summary>Lista de productos y cantidades de la orden.</summary>
        public List<CreateOrderItemRequest> Items { get; set; }
    }
}
