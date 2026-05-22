using Dapper;
using Microsoft.Data.Sqlite;
using Users.API.DTOs;
using Users.API.Exceptions;
using Users.API.Models;

namespace Users.API.Services;

public class UserService(IConfiguration config)
{
    private SqliteConnection CreateConnection()
    {
        var connectionString = config.GetConnectionString("DefaultConnection")
            ?? "Data Source=users.db";
        return new SqliteConnection(connectionString);
    }

    public async Task<UserResponse> RegisterAsync(RegisterRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Nombre) ||
            string.IsNullOrWhiteSpace(request.Apellido) ||
            string.IsNullOrWhiteSpace(request.Email) ||
            string.IsNullOrWhiteSpace(request.Password))
            throw new ValidationException("USR-002", "Los datos del usuario son inválidos.");

        if (!request.Email.Contains('@'))
            throw new ValidationException("USR-002", "Los datos del usuario son inválidos.");

        using var conn = CreateConnection();

        var exists = await conn.QueryFirstOrDefaultAsync<int>(
            "SELECT COUNT(1) FROM users WHERE email = @email",
            new { email = request.Email });

        if (exists > 0)
            throw new BusinessRuleException("USR-001", $"El email '{request.Email}' ya está registrado.");

        var id = Guid.NewGuid();
        var fechaRegistro = DateTime.UtcNow;
        var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

        await conn.ExecuteAsync(
            """
            INSERT INTO users (id, nombre, apellido, email, password_hash, fecha_registro, activo, intentos_fallidos)
            VALUES (@id, @nombre, @apellido, @email, @passwordHash, @fechaRegistro, 1, 0)
            """,
            new
            {
                id = id.ToString(),
                nombre = request.Nombre,
                apellido = request.Apellido,
                email = request.Email,
                passwordHash,
                fechaRegistro = fechaRegistro.ToString("o")
            });

        return new UserResponse
        {
            Id = id,
            Nombre = request.Nombre,
            Apellido = request.Apellido,
            Email = request.Email,
            FechaRegistro = fechaRegistro,
            Activo = true
        };
    }

    public async Task<LoginResponse> LoginAsync(LoginRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
            throw new ValidationException("USR-002", "Los datos del usuario son inválidos.");

        using var conn = CreateConnection();

        var row = await conn.QueryFirstOrDefaultAsync<dynamic>(
            """
            SELECT id AS Id, nombre AS Nombre, apellido AS Apellido, email AS Email,
                   password_hash AS PasswordHash, fecha_registro AS FechaRegistro,
                   activo AS Activo, intentos_fallidos AS IntentosFallidos
            FROM users WHERE email = @email
            """,
            new { email = request.Email });

        if (row == null)
            throw new BusinessRuleException("USR-003", "Credenciales incorrectas.");

        var user = new User
        {
            Id = Guid.Parse((string)row.Id),
            Nombre = (string)row.Nombre,
            Apellido = (string)row.Apellido,
            Email = (string)row.Email,
            PasswordHash = (string)row.PasswordHash,
            FechaRegistro = DateTime.Parse((string)row.FechaRegistro, null, System.Globalization.DateTimeStyles.RoundtripKind),
            Activo = (long)row.Activo == 1,
            IntentosFallidos = (int)(long)row.IntentosFallidos
        };

        if (!user.Activo && user.IntentosFallidos >= 3)
            throw new BusinessRuleException("USR-004", "Su cuenta fue bloqueada por superar el máximo de intentos fallidos. Contacte a soporte.");

        if (!user.Activo)
            throw new BusinessRuleException("USR-005", "Su cuenta fue suspendida por razones de seguridad. Contacte a soporte.");

        if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            var nuevoIntentos = user.IntentosFallidos + 1;
            var bloquear = nuevoIntentos >= 3;

            await conn.ExecuteAsync(
                "UPDATE users SET intentos_fallidos = @intentos, activo = @activo WHERE id = @id",
                new { intentos = nuevoIntentos, activo = bloquear ? 0 : 1, id = user.Id.ToString() });

            throw new BusinessRuleException("USR-003", "Credenciales incorrectas.");
        }

        await conn.ExecuteAsync(
            "UPDATE users SET intentos_fallidos = 0 WHERE id = @id",
            new { id = user.Id.ToString() });

        return new LoginResponse
        {
            Id = user.Id,
            Nombre = user.Nombre,
            Apellido = user.Apellido,
            Email = user.Email
        };
    }
}
