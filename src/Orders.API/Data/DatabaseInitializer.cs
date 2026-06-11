using Dapper;
using Microsoft.Data.Sqlite;

namespace Orders.API.Data;

public class DatabaseInitializer(IConfiguration config, ILogger<DatabaseInitializer> logger)
{
    private readonly IConfiguration _config = config;
    private readonly ILogger<DatabaseInitializer> _logger = logger; 

    public void Initialize()
    {
        var connectionString = _config.GetConnectionString("DefaultConnection")
            ?? "Data Source=app.db";

        using var connection = new SqliteConnection(connectionString);
        connection.Open();

        connection.Execute("""
                CREATE TABLE IF NOT EXISTS orders (
                id             TEXT PRIMARY KEY,
                usuario_id     TEXT NOT NULL,
                total          REAL NOT NULL, 
                estado         TEXT NOT NULL,
                fecha_creacion TEXT NOT NULL DEFAULT (datetime('now'))
                );
                """);

        connection.Execute("""
                CREATE TABLE IF NOT EXISTS order_items (                   
                 order_id        TEXT NOT NULL,
                 producto_id     TEXT NOT NULL,
                 cantidad        INTEGER NOT NULL,
                 precio_unitario REAL NOT NULL,
                 PRIMARY KEY (order_id, producto_id), 
                 FOREIGN KEY (order_id) REFERENCES orders(id)
                );
                """);

        _logger.LogInformation("SQLite inicializado correctamente → {db}", connectionString);
    }
 }


