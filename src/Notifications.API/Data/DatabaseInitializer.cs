using Dapper;
using Microsoft.Data.Sqlite;

namespace Notifications.API.Data
{
    public class DatabaseInitializer(IConfiguration config, ILogger<DatabaseInitializer> logger)
    {
        public void Initialize()
        {
            var connectionString = config.GetConnectionString("DefaultConnection")
                ?? "Data Source=notifications.db";

            using var connection = new SqliteConnection(connectionString);
            connection.Open();

            connection.Execute("""
            CREATE TABLE IF NOT EXISTS notifications (
                id          TEXT PRIMARY KEY,
                usuario_id  TEXT NOT NULL,
                mensaje     TEXT NOT NULL,
                tipo        TEXT NOT NULL,
                estado      TEXT NOT NULL,
                fecha_envio TEXT NOT NULL DEFAULT (datetime('now'))
            );
            """);

            logger.LogInformation("SQLite inicializado correctamente → {db}", connectionString);
        }
    }

}
