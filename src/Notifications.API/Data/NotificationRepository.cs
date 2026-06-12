using Dapper;
using Microsoft.Data.Sqlite;
using Notifications.API.Models;
using System.Globalization;

namespace Notifications.API.Data;

public class NotificationRepository(IConfiguration config)
{
    private SqliteConnection CreateConnection() =>
        new(config.GetConnectionString("DefaultConnection") ?? "Data Source=notifications.db");

    public async Task<Notification> CreateAsync(Notification notification)
    {
        using var conn = CreateConnection();

        var fechaEnvio = DateTime.UtcNow;
        await conn.ExecuteAsync("""
        INSERT INTO notifications (id, usuario_id, mensaje, tipo, estado, fecha_envio)
        VALUES (@Id, @UsuarioId, @Mensaje, @Tipo, @Estado, @FechaEnvio)
        """, new
        {
            Id = notification.Id.ToString(),
            UsuarioId = notification.UsuarioId.ToString(),
            notification.Mensaje,
            notification.Tipo,
            notification.Estado,
            FechaEnvio = fechaEnvio.ToString("o")
        });

        notification.FechaEnvio = fechaEnvio;
        return notification;
    }

    public async Task<IEnumerable<Notification>> GetByUserIdAsync(Guid usuarioId)
    {
        using var conn = CreateConnection();

        var rows = await conn.QueryAsync<dynamic>("""
        SELECT id AS Id, usuario_id AS UsuarioId, mensaje AS Mensaje,
               tipo AS Tipo, estado AS Estado, fecha_envio AS FechaEnvio
        FROM notifications
        WHERE usuario_id = @UsuarioId
        ORDER BY fecha_envio DESC
        """, new { UsuarioId = usuarioId.ToString() });

        var notifications = new List<Notification>();
        foreach (var row in rows)
        {
            notifications.Add(new Notification
            {
                Id = Guid.Parse((string)row.Id),
                UsuarioId = Guid.Parse((string)row.UsuarioId),
                Mensaje = (string)row.Mensaje,
                Tipo = (string)row.Tipo,
                Estado = (string)row.Estado,
                FechaEnvio = DateTime.Parse((string)row.FechaEnvio, CultureInfo.InvariantCulture)
            });
        }
        return notifications;
    }

    private async Task<Notification?> GetByIdAsync(Guid id)
    {
        using var conn = CreateConnection();

        var row = await conn.QueryFirstOrDefaultAsync<dynamic>("""
        SELECT id AS Id, usuario_id AS UsuarioId, mensaje AS Mensaje,
               tipo AS Tipo, estado AS Estado, fecha_envio AS FechaEnvio
        FROM notifications
        WHERE id = @Id
        """, new { Id = id.ToString() });

        if (row == null) return null;

        return new Notification
        {
            Id = Guid.Parse((string)row.Id),
            UsuarioId = Guid.Parse((string)row.UsuarioId),
            Mensaje = (string)row.Mensaje,
            Tipo = (string)row.Tipo,
            Estado = (string)row.Estado,
            FechaEnvio = DateTime.Parse((string)row.FechaEnvio, CultureInfo.InvariantCulture)
        };
    }
}
