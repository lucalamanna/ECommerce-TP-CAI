namespace Notifications.API.Models;

/// <summary>
/// Representa una notificación enviada a un usuario.
/// </summary>
public class Notification
{
    /// <summary>Identificador único de la notificación.</summary>
    /// <example>11112222-3333-4444-5555-666677778888</example>
    public Guid Id { get; set; }

    /// <summary>Identificador del usuario destinatario.</summary>
    /// <example>a1b2c3d4-0000-0000-0000-111122223333</example>
    public Guid UsuarioId { get; set; }

    /// <summary>Contenido del mensaje. Máximo 500 caracteres.</summary>
    /// <example>Su orden #f1e2d3c4 fue confirmada.</example>
    public string Mensaje { get; set; } = string.Empty;

    /// <summary>Canal de envío. Valores posibles: Email, Push, SMS.</summary>
    /// <example>Email</example>
    public string Tipo { get; set; } = string.Empty;

    /// <summary>Estado de la notificación. Valores posibles: Pendiente, Enviada, Fallida.</summary>
    /// <example>Enviada</example>
    public string Estado { get; set; } = string.Empty;

    /// <summary>Fecha y hora de envío. Asignada automáticamente.</summary>
    /// <example>2024-03-10T12:01:00Z</example>
    public DateTime FechaEnvio { get; set; }
}