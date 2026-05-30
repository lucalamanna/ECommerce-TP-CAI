namespace Users.API.DTOs;

/// <summary>
/// Datos del usuario devueltos al registrarse.
/// </summary>
public class UserResponse
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

    /// <summary>Email del usuario.</summary>
    /// <example>maria@email.com</example>
    public string Email { get; set; } = string.Empty;

    /// <summary>Fecha de registro del usuario.</summary>
    /// <example>2024-03-10T09:00:00Z</example>
    public DateTime FechaRegistro { get; set; }

    /// <summary>Indica si el usuario está activo.</summary>
    /// <example>true</example>
    public bool Activo { get; set; }
}