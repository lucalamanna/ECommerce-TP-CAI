using Microsoft.Data.Sqlite;
using Dapper;

namespace Products.API.Services;

public class DatabaseInitializer(IConfiguration config)
{
    private readonly IConfiguration _config = config;

    public void Initialize()
    {
        var connectionString = _config.GetConnectionString("DefaultConnection")
            ?? "Data Source=app.db";

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
    }
}