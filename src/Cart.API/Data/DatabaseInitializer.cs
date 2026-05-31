using Dapper;
using Microsoft.Data.Sqlite;

namespace Cart.API.Data;

public class DatabaseInitializer(IConfiguration config, ILogger<DatabaseInitializer> logger)
{
    public void Initialize()
    {
        var connectionString = config.GetConnectionString("DefaultConnection")
            ?? "Data Source=cart.db";
        using var connection = new SqliteConnection(connectionString);
        connection.Open();
        connection.Execute("""
            CREATE TABLE IF NOT EXISTS carts (
                usuario_id TEXT PRIMARY KEY,
                fecha_actualizacion TEXT NOT NULL
            );

            CREATE TABLE IF NOT EXISTS cart_items (
                usuario_id TEXT NOT NULL,
                producto_id TEXT NOT NULL,
                cantidad INTEGER NOT NULL DEFAULT 1,
                PRIMARY KEY (usuario_id, producto_id),
                FOREIGN KEY (usuario_id) REFERENCES carts(usuario_id)
            );
        """);
        logger.LogInformation("SQLite inicializado correctamente → {db}", connectionString);
    }
}