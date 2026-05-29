using Microsoft.Data.Sqlite;
using Dapper;

namespace Products.API.Data;

public class DatabaseInitializer(IConfiguration config, ILogger<DatabaseInitializer> logger)
{
    public void Initialize()
    {
        var connectionString = config.GetConnectionString("DefaultConnection")
            ?? "Data Source=products.db";

        using var connection = new SqliteConnection(connectionString);
        connection.Open();

        connection.Execute("""
            CREATE TABLE IF NOT EXISTS products (
                id TEXT PRIMARY KEY,
                nombre TEXT NOT NULL,
                descripcion TEXT,
                precio REAL NOT NULL,
                stock INTEGER NOT NULL DEFAULT 0,
                categoria TEXT NOT NULL,
                fecha_creacion TEXT NOT NULL
            );
            """);

        logger.LogInformation("SQLite inicializado correctamente → {db}", connectionString);
    }
}