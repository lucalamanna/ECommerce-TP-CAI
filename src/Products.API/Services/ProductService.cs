using Microsoft.Data.Sqlite;
using Dapper;
using Products.API.Models;
using Products.API.DTOs;
using Products.API.Exceptions;

namespace Products.API.Services;

public class ProductService(IConfiguration config)
{
    private SqliteConnection CreateConnection()
    {
        var connectionString = config.GetConnectionString("DefaultConnection")
            ?? "Data Source=products.db";
        return new SqliteConnection(connectionString);
    }

    public async Task<IEnumerable<Product>> GetAllAsync(string? categoria, string? nombre)
    {
        using var conn = CreateConnection();
        var sql = "SELECT id AS Id, nombre AS Nombre, descripcion AS Descripcion, precio AS Precio, stock AS Stock, categoria AS Categoria, fecha_creacion AS FechaCreacion FROM products WHERE 1=1";
        if (!string.IsNullOrEmpty(categoria)) sql += " AND categoria = @categoria";
        if (!string.IsNullOrEmpty(nombre)) sql += " AND nombre LIKE @nombre";
        return await conn.QueryAsync<Product>(sql, new { categoria, nombre = $"%{nombre}%" });
    }

    public async Task<Product> GetByIdAsync(Guid id)
    {
        using var conn = CreateConnection();
        var product = await conn.QueryFirstOrDefaultAsync<Product>(
            "SELECT id AS Id, nombre AS Nombre, descripcion AS Descripcion, precio AS Precio, stock AS Stock, categoria AS Categoria, fecha_creacion AS FechaCreacion FROM products WHERE id = @id",
            new { id = id.ToString() });

        if (product == null)
            throw new NotFoundException("PRD-001", "Producto no encontrado.");

        return product;
    }

    public async Task<Product> CreateAsync(CreateProductRequest request)
    {
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

        using var conn = CreateConnection();
        await conn.ExecuteAsync(
            "DELETE FROM products WHERE id = @id",
            new { id = id.ToString() });
    }
}