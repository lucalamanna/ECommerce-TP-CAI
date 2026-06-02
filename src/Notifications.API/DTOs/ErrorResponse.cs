namespace Notifications.API.DTOs
{
    public class ErrorResponse
    {
        /// <summary>URI que identifica el tipo de error según RFC.</summary>
        /// <example>https://tools.ietf.org/html/rfc7231#section-6.5.4</example>
        public string Type { get; set; } = string.Empty;

        /// <summary>Título del error.</summary>
        /// <example>Not Found</example>
        public string Title { get; set; } = string.Empty;

        /// <summary>Código HTTP del error.</summary>
        /// <example>404</example>
        public int Status { get; set; }

        /// <summary>Descripción detallada del error.</summary>
        /// <example>El recurso solicitado no fue encontrado.</example>
        public string Detail { get; set; } = string.Empty;

        /// <summary>Path del endpoint que generó el error.</summary>
        /// <example>/api/orders/f1e2d3c4-0000-0000-0000-aabbccddeeff</example>
        public string Instance { get; set; } = string.Empty;

        /// <summary>Código de error del catálogo de Orders API.</summary>
        /// <example>ORD-001</example>
        public string ErrorCode { get; set; } = string.Empty;

        /// <summary>Mensaje descriptivo del error.</summary>
        /// <example>Orden no encontrada.</example>
        public string ErrorMessage { get; set; } = string.Empty;

        /// <summary>Identificador único del request para trazabilidad.</summary>
        /// <example>8f2c1a9e-4b3d-4562-b3fc-2c963f66afa6</example>
        public string? CorrelationId { get; set; }
    }
}
