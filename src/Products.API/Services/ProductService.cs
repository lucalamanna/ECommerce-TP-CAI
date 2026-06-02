using Microsoft.Data.Sqlite;
using Dapper;
using Products.API.Models;
using Products.API.DTOs;
using Products.API.Exceptions;

namespace Products.API.Services;

public class ProductService(IConfiguration config, IHttpClientFactory httpClientFactory)
{
    private SqliteConnection CreateConnection()
    {
        var connectionString = config.GetConnectionString("DefaultConnection")
            ?? "Data Source=products.db";
        return new SqliteConnection(connectionString);
    }

    private static void ValidarProducto(string nombre, string? descripcion, decimal precio, int? stock, string categoria)
    {
        var errores = new List<string>();

        if (string.IsNullOrWhiteSpace(nombre))
            errores.Add("El campo 'Nombre' es requerido");
        else if (nombre.Length > 100)
            errores.Add("El campo 'Nombre' no puede superar los 100 caracteres");

        if (descripcion != null && descripcion.Length > 500)
            errores.Add("El campo 'Descripcion' no puede superar los 500 caracteres");

        if (precio <= 0)
            errores.Add("El campo 'Precio' debe ser mayor a 0");

        if (stock == null)
            errores.Add("El campo 'Stock' es requerido");
        else if (stock < 0)
            errores.Add("El campo 'Stock' debe ser mayor o igual a 0");

        if (string.IsNullOrWhiteSpace(categoria))
            errores.Add("El campo 'Categoria' es requerido");

        if (errores.Count > 0)
            throw new ValidationException("PRD-002", string.Join("; ", errores));
    }

    public async Task<IEnumerable<Product>> GetAllAsync(string? categoria, string? nombre)
    {
        using var conn = CreateConnection();
        var sql = "SELECT id AS Id, nombre AS Nombre, descripcion AS Descripcion, precio AS Precio, stock AS Stock, categoria AS Categoria, fecha_creacion AS FechaCreacion FROM products WHERE 1=1";
        if (!string.IsNullOrEmpty(categoria)) sql += " AND categoria = @categoria";
        if (!string.IsNullOrEmpty(nombre)) sql += " AND nombre LIKE @nombre";
        var rows = await conn.QueryAsync<dynamic>(sql, new { categoria, nombre = $"%{nombre}%" });
        return rows.Select(row => new Product
        {
            Id = Guid.Parse((string)row.Id),
            Nombre = (string)row.Nombre,
            Descripcion = (string)row.Descripcion,
            Precio = (decimal)(double)row.Precio,
            Stock = (int)(long)row.Stock,
            Categoria = (string)row.Categoria,
            FechaCreacion = DateTime.Parse((string)row.FechaCreacion, null, System.Globalization.DateTimeStyles.RoundtripKind)
        });
    }

    public async Task<Product> GetByIdAsync(Guid id)
    {
        using var conn = CreateConnection();
        var row = await conn.QueryFirstOrDefaultAsync<dynamic>(
            "SELECT id AS Id, nombre AS Nombre, descripcion AS Descripcion, precio AS Precio, stock AS Stock, categoria AS Categoria, fecha_creacion AS FechaCreacion FROM products WHERE id = @id",
            new { id = id.ToString() });

        if (row == null)
            throw new NotFoundException("PRD-001", "Producto no encontrado.");

        return new Product
        {
            Id = Guid.Parse((string)row.Id),
            Nombre = (string)row.Nombre,
            Descripcion = (string)row.Descripcion,
            Precio = (decimal)(double)row.Precio,
            Stock = (int)(long)row.Stock,
            Categoria = (string)row.Categoria,
            FechaCreacion = DateTime.Parse((string)row.FechaCreacion, null, System.Globalization.DateTimeStyles.RoundtripKind)
        };
    }

    public async Task<Product> CreateAsync(CreateProductRequest request)
    {
        ValidarProducto(request.Nombre, request.Descripcion, request.Precio, request.Stock, request.Categoria);

        using var conn = CreateConnection();

        var exists = await conn.QueryFirstOrDefaultAsync<int>(
            "SELECT COUNT(1) FROM products WHERE nombre = @nombre AND categoria = @categoria",
            new { nombre = request.Nombre, categoria = request.Categoria });

        if (exists > 0)
            throw new BusinessRuleException("PRD-003", $"Ya existe un producto con ese nombre en la categoría '{request.Categoria}'.");

        var id = Guid.NewGuid();
        var fechaCreacion = DateTime.UtcNow;

        await conn.ExecuteAsync(
            "INSERT INTO products (id, nombre, descripcion, precio, stock, categoria, fecha_creacion) VALUES (@id, @nombre, @descripcion, @precio, @stock, @categoria, @fechaCreacion)",
            new { id = id.ToString(), nombre = request.Nombre, descripcion = request.Descripcion, precio = request.Precio, stock = request.Stock, categoria = request.Categoria, fechaCreacion = fechaCreacion.ToString("o") });

        return await GetByIdAsync(id);
    }

    public async Task<Product> UpdateAsync(Guid id, UpdateProductRequest request)
    {
        ValidarProducto(request.Nombre, request.Descripcion, request.Precio, request.Stock, request.Categoria);

        await GetByIdAsync(id);

        using var conn = CreateConnection();
        await conn.ExecuteAsync(
            "UPDATE products SET nombre = @nombre, descripcion = @descripcion, precio = @precio, stock = @stock, categoria = @categoria WHERE id = @id",
            new { id = id.ToString(), nombre = request.Nombre, descripcion = request.Descripcion, precio = request.Precio, stock = request.Stock, categoria = request.Categoria });

        return await GetByIdAsync(id);
    }

    public async Task DeleteAsync(Guid id)
    {
        await GetByIdAsync(id);

        var client = httpClientFactory.CreateClient("OrdersApi");
        var response = await client.GetAsync($"/api/orders?productoId={id}");

        if (response.IsSuccessStatusCode)
        {
            var orders = await response.Content.ReadFromJsonAsync<IEnumerable<OrderSummary>>();
            var ordenesActivas = orders?.Where(o =>
                o.Estado == "Pendiente" ||
                o.Estado == "Confirmada").ToList();

            if (ordenesActivas != null && ordenesActivas.Any())
                throw new BusinessRuleException("PRD-004",
                    "El producto tiene órdenes activas y no puede eliminarse.");
        }

        using var conn = CreateConnection();
        await conn.ExecuteAsync(
            "DELETE FROM products WHERE id = @id",
            new { id = id.ToString() });
    }
}