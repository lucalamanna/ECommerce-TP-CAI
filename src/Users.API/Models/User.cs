namespace Users.API.Models;

/// <summary>
/// Representa un usuario del sistema.
/// </summary>
public class User
{
    /// <summary>Identificador único del usuario.</summary>
    /// <example>a1b2c3d4-0000-0000-0000-111122223333</example>
    public Guid Id { get; set; }

    /// <summary>Nombre del usuario.</summary>
    /// <example>María</example>
    public string Nombre { get; set; } = string.Empty;

    /// <summary>Apellido del usuario.</summary>
    /// <example>González</example>
    public string Apellido { get; set; } = string.Empty;

    /// <summary>Email del usuario. Debe ser único en el sistema.</summary>
    /// <example>maria@email.com</example>
    public string Email { get; set; } = string.Empty;

    /// <summary>Hash de la contraseña. Nunca se expone en responses.</summary>
    public string PasswordHash { get; set; } = string.Empty;

    /// <summary>Fecha de registro. Asignada automáticamente.</summary>
    /// <example>2024-03-10T09:00:00Z</example>
    public DateTime FechaRegistro { get; set; }

    /// <summary>Indica si el usuario está activo. false cuando está bloqueado.</summary>
    /// <example>true</example>
    public bool Activo { get; set; }

    /// <summary>Cantidad de intentos de login fallidos consecutivos.</summary>
    /// <example>0</example>
    public int IntentosFallidos { get; set; }
}