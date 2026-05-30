namespace Orders.API.DTOs
{
    /// <summary>Resultado de la actualización de estado de una orden.</summary>
    public class UpdateOrderStatusResponse
    {
        /// <summary>Identificador de la orden actualizada.</summary>
        /// <example>f1e2d3c4-0000-0000-0000-aabbccddeeff</example>
        public Guid Id { get; set; }

        /// <summary>Nuevo estado de la orden.</summary>
        /// <example>Confirmada</example>
        public string Estado { get; set; }

        /// <summary>Fecha de la actualización (UTC).</summary>
        /// <example>2026-05-29T15:00:00Z</example>
        public DateTime FechaActualizacion { get; set; }
    }
}
