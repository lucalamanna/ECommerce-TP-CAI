namespace Orders.API.Models
{
       /// <summary>Entidad de dominio que representa una orden de compra.</summary>
    public class Order
    {
        /// <summary>Identificador único de la orden.</summary>
        public Guid Id { get; set; }

        /// <summary>Identificador del usuario que realizó la orden.</summary>
        public Guid UsuarioId { get; set; }

        /// <summary>Lista de productos incluidos en la orden.</summary>
        public List<OrderItem> Items { get; set; } = new();

        /// <summary>Monto total calculado de la orden.</summary>
        public decimal Total { get; set; }

        /// <summary>Estado actual de la orden (Pendiente, Confirmada, Enviada, Entregada, Cancelada).</summary>
        public string Estado { get; set; } = string.Empty;

        /// <summary>Fecha y hora de creación de la orden (UTC).</summary>
        public DateTime FechaCreacion { get; set; }
    }
    
}
