using Dapper;
using Microsoft.Data.Sqlite;

namespace Users.API.Data;

public class DatabaseInitializer(IConfiguration config, ILogger<DatabaseInitializer> logger)
{
    public void Initialize()
    {
        var connectionString = config.GetConnectionString("DefaultConnection")
            ?? "Data Source=users.db";
        using var connection = new SqliteConnection(connectionString);
        connection.Open();
        connection.Execute("""
            CREATE TABLE IF NOT EXISTS users (
                id TEXT PRIMARY KEY,
                nombre TEXT NOT NULL,
                apellido TEXT NOT NULL,
                email TEXT NOT NULL UNIQUE,
                password_hash TEXT NOT NULL,
                fecha_registro TEXT NOT NULL,
                activo INTEGER NOT NULL DEFAULT 1,
                intentos_fallidos INTEGER NOT NULL DEFAULT 0
            );
        """);
        logger.LogInformation("SQLite inicializado correctamente → {db}", connectionString);
    }
}