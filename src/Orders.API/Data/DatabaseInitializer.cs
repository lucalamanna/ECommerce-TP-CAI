using Dapper;
using Microsoft.Data.Sqlite;

namespace Orders.API.Data
{
    public class DatabaseInitializer 
    {
        private readonly IConfiguration _config;
        private readonly ILogger<DatabaseInitializer> _logger;

        public DatabaseInitializer(IConfiguration config, ILogger<DatabaseInitializer> logger)
        {
            _config = config;
            _logger = logger;
        }

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
                total          REAL NOT NULL DEFAULT 0,
                estado         TEXT NOT NULL DEFAULT 'Pendiente',
                fecha_creacion TEXT NOT NULL DEFAULT (datetime('now'))
            );

            CREATE TABLE IF NOT EXISTS order_items (
                id              TEXT PRIMARY KEY,
                order_id        TEXT NOT NULL,
                producto_id     TEXT NOT NULL,
                cantidad        INTEGER NOT NULL,
                precio_unitario REAL NOT NULL,
                FOREIGN KEY (order_id) REFERENCES orders(id)
            );
        """);

            _logger.LogInformation("SQLite inicializado correctamente → {db}", connectionString);
        }

    }

 }
