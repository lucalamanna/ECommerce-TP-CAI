using Dapper;
using Microsoft.Data.Sqlite;
using Orders.API.Models;
using Order = Orders.API.Models.Order;

namespace Orders.API.Data
{
    public class OrderRepository
    {
        private readonly IConfiguration _config;

        public OrderRepository(IConfiguration config) => _config = config;

        private SqliteConnection CreateConnection() =>
            new(_config.GetConnectionString("DefaultConnection") ?? "Data Source=app.db");

        // --- GET ALL ---
        public async Task<IEnumerable<Order>> GetAllAsync(Guid? usuarioId)
        {       
            using var conn = CreateConnection();

                var orders = await conn.QueryAsync<Order>("""
                SELECT id AS Id, usuario_id AS UsuarioId, total AS Total,
                estado AS Estado, fecha_creacion AS FechaCreacion
                FROM orders
                WHERE (@UsuarioId IS NULL OR usuario_id = @UsuarioId)
                ORDER BY fecha_creacion DESC
                """, new { UsuarioId = usuarioId?.ToString() });

            foreach (var order in orders)
            {
                order.Items = (await conn.QueryAsync<OrderItem>("""
            SELECT producto_id AS ProductoId, cantidad AS Cantidad,
                   precio_unitario AS PrecioUnitario
            FROM order_items
            WHERE order_id = @OrderId
            """, new { OrderId = order.Id.ToString() })).ToList();
            }

            return orders;
        }

        // --- GET BY ID ---
        public async Task<Order?> GetByIdAsync(Guid id)
        {
            using var conn = CreateConnection();

            var order = await conn.QuerySingleOrDefaultAsync<Order>("""
        SELECT id AS Id, usuario_id AS UsuarioId, total AS Total,
               estado AS Estado, fecha_creacion AS FechaCreacion
        FROM orders
        WHERE id = @Id
        """, new { Id = id.ToString() });

            if (order is not null)
            {
                order.Items = (await conn.QueryAsync<OrderItem>("""
            SELECT producto_id AS ProductoId, cantidad AS Cantidad,
                   precio_unitario AS PrecioUnitario
            FROM order_items
            WHERE order_id = @OrderId
            """, new { OrderId = order.Id.ToString() })).ToList();
            }

            return order;
        }

        // --- CREATE ---
        public async Task<Order> CreateAsync(Order order)
        {
            using var conn = CreateConnection();

            await conn.ExecuteAsync("""
        INSERT INTO orders (id, usuario_id, total, estado)
        VALUES (@Id, @UsuarioId, @Total, @Estado)
        """, new
            {
                Id = order.Id.ToString(),
                UsuarioId = order.UsuarioId.ToString(),
                order.Total,
                order.Estado
            });

            foreach (var item in order.Items)
            {
                await conn.ExecuteAsync("""
            INSERT INTO order_items (order_id, producto_id, cantidad, precio_unitario)
            VALUES (@OrderId, @ProductoId, @Cantidad, @PrecioUnitario)
            """, new
                {
                    OrderId = order.Id.ToString(),
                    ProductoId = item.ProductoId.ToString(),
                    item.Cantidad,
                    item.PrecioUnitario
                });
            }

            return (await GetByIdAsync(order.Id))!;
        }

        // --- UPDATE STATUS ---
        public async Task<Order?> UpdateStatusAsync(Guid id, string estado)
        {
            using var conn = CreateConnection();

            var rows = await conn.ExecuteAsync("""
        UPDATE orders
        SET estado = @Estado
        WHERE id = @Id
        """, new { Estado = estado, Id = id.ToString() });

            if (rows == 0) return null;

            return await GetByIdAsync(id);
        }
    }
}
