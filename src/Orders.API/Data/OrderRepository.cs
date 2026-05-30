using Dapper;
using Microsoft.Data.Sqlite;
using Orders.API.Models;
using Order = Orders.API.Models.Order;
using System.Globalization;

namespace Orders.API.Data
{
    public class OrderRepository
    {
        private readonly IConfiguration _config;

        public OrderRepository(IConfiguration config) => _config = config;

        private SqliteConnection CreateConnection() =>
            new(_config.GetConnectionString("DefaultConnection") ?? "Data Source=app.db");

        // --- GET ALL ---
        public async Task<IEnumerable<Order>> GetAllAsync(Guid? usuarioId, Guid? productoId)
        {       
            using var conn = CreateConnection();

                var rows = await conn.QueryAsync< dynamic> ("""
                SELECT id AS Id, usuario_id AS UsuarioId, total AS Total,
                estado AS Estado, fecha_creacion AS FechaCreacion
                FROM orders o
                WHERE (@UsuarioId IS NULL OR usuario_id = @UsuarioId)
                    AND (@ProductoId IS NULL OR EXISTS (
                          SELECT 1 FROM order_items oi
                           WHERE oi.order_id = o.id AND oi.producto_id = @ProductoId ))
                ORDER BY fecha_creacion DESC
                """, new { UsuarioId = usuarioId?.ToString(), 
                    ProductoId = productoId?.ToString() });

            var orders = rows.Select(row => new Order
            {
                Id = Guid.Parse((string)row.Id),
                UsuarioId = Guid.Parse((string)row.UsuarioId),
                Total = (decimal)(double)row.Total,
                Estado = (string)row.Estado,
                FechaCreacion = DateTime.Parse((string)row.FechaCreacion, CultureInfo.InvariantCulture)
            }).ToList();

            foreach (var order in orders)
            {
                var itemRows = (await conn.QueryAsync<dynamic>("""
            SELECT producto_id AS ProductoId, cantidad AS Cantidad,
                   precio_unitario AS PrecioUnitario
            FROM order_items
            WHERE order_id = @OrderId
            """, new { OrderId = order.Id.ToString() })).ToList();

                order.Items = itemRows.Select(row => new OrderItem
                {
                    ProductoId = Guid.Parse((string)row.ProductoId),
                    Cantidad = (int)(long)row.Cantidad,
                    PrecioUnitario = (decimal)(double)row.PrecioUnitario
                }).ToList();
            }
             
            return orders;
        }

        // --- GET BY ID ---
        public async Task<Order?> GetByIdAsync(Guid id)
        {
            using var conn = CreateConnection();

            var row = await conn.QuerySingleOrDefaultAsync<dynamic>("""
        SELECT id AS Id, usuario_id AS UsuarioId, total AS Total,
               estado AS Estado, fecha_creacion AS FechaCreacion
        FROM orders
        WHERE id = @Id
        """, new { Id = id.ToString() });

            if (row == null) return null;

            var order = new Order
            {
                Id = Guid.Parse((string)row.Id),
                UsuarioId = Guid.Parse((string)row.UsuarioId),
                Total = (decimal)(double)row.Total,
                Estado = (string)row.Estado,
                FechaCreacion = DateTime.Parse((string)row.FechaCreacion,
                                CultureInfo.InvariantCulture)
            };

            var itemRows = await conn.QueryAsync<dynamic>("""
            SELECT producto_id AS ProductoId, cantidad AS Cantidad,
                   precio_unitario AS PrecioUnitario
            FROM order_items
            WHERE order_id = @OrderId
            """, new { OrderId = order.Id.ToString() });

            order.Items = itemRows.Select(row => new OrderItem
            {
                ProductoId = Guid.Parse((string)row.ProductoId),
                Cantidad = (int)(long)row.Cantidad,
                PrecioUnitario = (decimal)(double)row.PrecioUnitario
            }).ToList();

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

        //DELETE 
        public async Task<bool> DeleteAsync(Guid id)
        {
            using var conn = CreateConnection();

            await conn.ExecuteAsync("""
        DELETE FROM order_items
        WHERE order_id = @Id
        """, new { Id = id.ToString() });

            var rows = await conn.ExecuteAsync("""
        DELETE FROM orders
        WHERE id = @Id
        """, new { Id = id.ToString() });

            return rows > 0;
        }
    }
}
