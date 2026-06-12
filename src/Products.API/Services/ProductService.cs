using Microsoft.Data.Sqlite;
using Dapper;
using Products.API.Models;
using Products.API.DTOs;
using Products.API.Exceptions;

namespace Products.API.Services;

public class ProductService(IConfiguration config, IHttpClientFactory httpClientFactory, ILogger<ProductService> logger)
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
        {
            var mensaje = "";
            for (int i = 0; i < errores.Count; i++)
            {
                if (i > 0) mensaje += "; ";
                mensaje += errores[i];
            }
            throw new ValidationException("PRD-002", mensaje);
        }
    }

    private static Product MapRow(dynamic row)
    {
        return new Product
        {
            Id = Guid.Parse((string)row.Id),
            Nombre = (string)row.Nombre,
            Descripcion = (string?)row.Descripcion,
            Precio = (decimal)(double)row.Precio,
            Stock = (int)(long)row.Stock,
            Categoria = (string)row.Categoria,
            FechaCreacion = DateTime.Parse((string)row.FechaCreacion, null, System.Globalization.DateTimeStyles.RoundtripKind)
        };
    }

    private const string SelectColumns =
        "SELECT id AS Id, nombre AS Nombre, descripcion AS Descripcion, " +
        "precio AS Precio, stock AS Stock, categoria AS Categoria, " +
        "fecha_creacion AS FechaCreacion FROM products";

    public async Task<IEnumerable<Product>> GetAllAsync(string? categoria, string? nombre)
    {
        logger.LogInformation("Obteniendo productos. Categoria: {Categoria}, Nombre: {Nombre}", categoria, nombre);

        using var conn = CreateConnection();
        var sql = $"{SelectColumns} WHERE 1=1";
        if (!string.IsNullOrEmpty(categoria)) sql += " AND categoria = @categoria";
        if (!string.IsNullOrEmpty(nombre)) sql += " AND nombre LIKE @nombre";
        var rows = await conn.QueryAsync<dynamic>(sql, new { categoria, nombre = $"%{nombre}%" });

        var products = new List<Product>();
        foreach (var row in rows)
        {
            products.Add(MapRow(row));
        }
        logger.LogInformation("Productos obtenidos. Cantidad: {Cantidad}", (int)products.Count);
        return products;
    }

    public async Task<Product> GetByIdAsync(Guid id)
    {
        logger.LogInformation("Obteniendo producto. Id: {Id}", id);

        using var conn = CreateConnection();
        var row = await conn.QueryFirstOrDefaultAsync<dynamic>(
            $"{SelectColumns} WHERE id = @id",
            new { id = id.ToString() });

        if (row == null)
        {
            logger.LogWarning("Producto no encontrado. ErrorCode: {ErrorCode}, Id: {Id}", "PRD-001", id);
            throw new NotFoundException("PRD-001", "Producto no encontrado.");
        }

        Product product = MapRow(row);
        logger.LogInformation("Producto encontrado. Id: {Id}, Nombre: {Nombre}",
            product.Id.ToString(), product.Nombre.ToString()); return product;
    }

    public async Task<Product> CreateAsync(CreateProductRequest request)
    {
        logger.LogInformation("Creando producto. Nombre: {Nombre}, Categoria: {Categoria}", request.Nombre, request.Categoria);

        ValidarProducto(request.Nombre, request.Descripcion, request.Precio, request.Stock, request.Categoria);

        using var conn = CreateConnection();

        var exists = await conn.QueryFirstOrDefaultAsync<int>(
            "SELECT COUNT(1) FROM products WHERE nombre = @nombre AND categoria = @categoria",
            new { nombre = request.Nombre, categoria = request.Categoria });

        if (exists > 0)
        {
            logger.LogWarning("Producto duplicado. ErrorCode: {ErrorCode}, Nombre: {Nombre}, Categoria: {Categoria}",
                "PRD-003", request.Nombre, request.Categoria);
            throw new BusinessRuleException("PRD-003", $"Ya existe un producto con ese nombre en la categoría '{request.Categoria}'.");
        }

        var id = Guid.NewGuid();
        var fechaCreacion = DateTime.UtcNow;

        await conn.ExecuteAsync(
            "INSERT INTO products (id, nombre, descripcion, precio, stock, categoria, fecha_creacion) VALUES (@id, @nombre, @descripcion, @precio, @stock, @categoria, @fechaCreacion)",
            new { id = id.ToString(), nombre = request.Nombre, descripcion = request.Descripcion, precio = request.Precio, stock = request.Stock, categoria = request.Categoria, fechaCreacion = fechaCreacion.ToString("o") });

        logger.LogInformation("Producto creado. Id: {Id}, Nombre: {Nombre}", id, request.Nombre);
        return await GetByIdAsync(id);
    }

    public async Task<Product> UpdateAsync(Guid id, UpdateProductRequest request)
    {
        logger.LogInformation("Actualizando producto. Id: {Id}", id);

        ValidarProducto(request.Nombre, request.Descripcion, request.Precio, request.Stock, request.Categoria);

        await GetByIdAsync(id);

        using var conn = CreateConnection();
        await conn.ExecuteAsync(
            "UPDATE products SET nombre = @nombre, descripcion = @descripcion, precio = @precio, stock = @stock, categoria = @categoria WHERE id = @id",
            new { id = id.ToString(), nombre = request.Nombre, descripcion = request.Descripcion, precio = request.Precio, stock = request.Stock, categoria = request.Categoria });

        logger.LogInformation("Producto actualizado. Id: {Id}, Nombre: {Nombre}", id, request.Nombre);
        return await GetByIdAsync(id);
    }

    public async Task DeleteAsync(Guid id)
    {
        logger.LogInformation("Eliminando producto. Id: {Id}", id);

        await GetByIdAsync(id);

        var client = httpClientFactory.CreateClient("OrdersApi");
        var response = await client.GetAsync($"/api/orders?productoId={id}");

        if (response.IsSuccessStatusCode)
        {
            var orders = await response.Content.ReadFromJsonAsync<IEnumerable<OrderSummary>>();
            var ordenesActivas = new List<OrderSummary>();
            if (orders != null)
            {
                foreach (var o in orders)
                {
                    if (o.Estado == "Pendiente" || o.Estado == "Confirmada")
                        ordenesActivas.Add(o);
                }
            }

            if (ordenesActivas.Count > 0)
            {
                logger.LogWarning("Producto con órdenes activas. ErrorCode: {ErrorCode}, Id: {Id}", "PRD-004", id);
                throw new BusinessRuleException("PRD-004",
                    "El producto tiene órdenes activas y no puede eliminarse.");
            }
        }

        using var conn = CreateConnection();
        await conn.ExecuteAsync(
            "DELETE FROM products WHERE id = @id",
            new { id = id.ToString() });

        logger.LogInformation("Producto eliminado. Id: {Id}", id);
    }
}