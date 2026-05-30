namespace Users.API.DTOs;

/// <summary>
/// Datos necesarios para registrar un nuevo usuario.
/// </summary>
public class RegisterRequest
{
    /// <summary>Nombre del usuario.</summary>
    /// <example>María</example>
    public string Nombre { get; set; } = string.Empty;

    /// <summary>Apellido del usuario.</summary>
    /// <example>González</example>
    public string Apellido { get; set; } = string.Empty;

    /// <summary>Email del usuario. Debe ser único.</summary>
    /// <example>maria@email.com</example>
    public string Email { get; set; } = string.Empty;

    /// <summary>Contraseña del usuario.</summary>
    /// <example>MiPassword123!</example>
    public string Password { get; set; } = string.Empty;
}