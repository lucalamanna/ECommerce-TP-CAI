namespace Orders.API.DTOs;

    /// <summary>Datos para actualizar el estado de una orden.</summary>
    public class UpdateOrderStatusRequest
    {
        /// <summary>Nuevo estado (Pendiente, Confirmada, Enviada, Entregada, Cancelada).</summary>
        /// <example>Confirmada</example>
        public string Estado { get; set; }
    }

