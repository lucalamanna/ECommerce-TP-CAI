namespace Orders.API.DTOs
{
    /// <summary>Representación de una orden.</summary>
    public class OrderResponse
    {
        /// <summary>Identificador único de la orden.</summary>
        /// <example>f1e2d3c4-0000-0000-0000-aabbccddeeff</example>
        public Guid Id { get; set; }

        /// <summary>Identificador del usuario dueño de la orden.</summary>
        /// <example>a1b2c3d4-0000-0000-0000-111122223333</example>
        public Guid UsuarioId { get; set; }

        /// <summary>Productos incluidos en la orden.</summary>
        public List<OrderItemResponse> Items { get; set; }

        /// <summary>Monto total de la orden.</summary>
        /// <example>3000.00</example>
        public decimal Total { get; set; }

        /// <summary>Estado actual de la orden.</summary>
        /// <example>Pendiente</example>
        public string Estado { get; set; }

        /// <summary>Fecha de creación de la orden (UTC).</summary>
        /// <example>2026-05-29T14:32:10Z</example>
        public DateTime FechaCreacion { get; set; }
    }
}
