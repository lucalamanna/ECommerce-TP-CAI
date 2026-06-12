using Cart.API.DTOs;
using Cart.API.Exceptions;
using Dapper;
using Microsoft.Data.Sqlite;

namespace Cart.API.Services;

public class CartService(IConfiguration config, IHttpClientFactory httpClientFactory)
{
    private SqliteConnection CreateConnection()
    {
        var connectionString = config.GetConnectionString("DefaultConnection")
            ?? "Data Source=cart.db";
        return new SqliteConnection(connectionString);
    }

    public async Task<CartResponse> GetCartAsync(Guid userId)
    {
        using var conn = CreateConnection();

        var cart = await conn.QueryFirstOrDefaultAsync<dynamic>(
            "SELECT usuario_id AS UsuarioId, fecha_actualizacion AS FechaActualizacion FROM carts WHERE usuario_id = @userId",
            new { userId = userId.ToString() });

        if (cart == null)
            throw new NotFoundException("CRT-001", $"No existe un carrito activo para el usuario '{userId}'.");

        var items = await conn.QueryAsync<dynamic>(
            "SELECT producto_id AS ProductoId, cantidad AS Cantidad FROM cart_items WHERE usuario_id = @userId",
            new { userId = userId.ToString() });

        var itemList = new List<CartItemResponse>();
        foreach (var i in items)
        {
            itemList.Add(new CartItemResponse
            {
                ProductoId = Guid.Parse((string)i.ProductoId),
                Cantidad = (int)(long)i.Cantidad
            });
        }

        return new CartResponse
        {
            UsuarioId = Guid.Parse((string)cart.UsuarioId),
            FechaActualizacion = DateTime.Parse((string)cart.FechaActualizacion, null, System.Globalization.DateTimeStyles.RoundtripKind),
            Items = itemList
        };
    }

    public async Task<CartResponse> AddItemAsync(Guid userId, AddItemRequest request)
    {
        if (request.Cantidad <= 0)
            throw new ValidationException("CRT-004", "La cantidad debe ser mayor a cero.");

        var product = await GetProductAsync(request.ProductoId);

        if (product == null)
            throw new NotFoundException("CRT-002", $"El producto '{request.ProductoId}' no fue encontrado.");

        if (product.Stock < request.Cantidad)
            throw new BusinessRuleException("CRT-003", $"Stock insuficiente. Disponible: {product.Stock}, solicitado: {request.Cantidad}.");

        using var conn = CreateConnection();
        var now = DateTime.UtcNow.ToString("o");

        var cartExists = await conn.QueryFirstOrDefaultAsync<int>(
            "SELECT COUNT(1) FROM carts WHERE usuario_id = @userId",
            new { userId = userId.ToString() });

        if (cartExists == 0)
            await conn.ExecuteAsync(
                "INSERT INTO carts (usuario_id, fecha_actualizacion) VALUES (@userId, @now)",
                new { userId = userId.ToString(), now });
        else
            await conn.ExecuteAsync(
                "UPDATE carts SET fecha_actualizacion = @now WHERE usuario_id = @userId",
                new { userId = userId.ToString(), now });

        var itemExists = await conn.QueryFirstOrDefaultAsync<int>(
            "SELECT COUNT(1) FROM cart_items WHERE usuario_id = @userId AND producto_id = @productoId",
            new { userId = userId.ToString(), productoId = request.ProductoId.ToString() });

        if (itemExists > 0)
            await conn.ExecuteAsync(
                "UPDATE cart_items SET cantidad = cantidad + @cantidad WHERE usuario_id = @userId AND producto_id = @productoId",
                new { userId = userId.ToString(), productoId = request.ProductoId.ToString(), cantidad = request.Cantidad });
        else
            await conn.ExecuteAsync(
                "INSERT INTO cart_items (usuario_id, producto_id, cantidad) VALUES (@userId, @productoId, @cantidad)",
                new { userId = userId.ToString(), productoId = request.ProductoId.ToString(), cantidad = request.Cantidad });

        return await GetCartAsync(userId);
    }

    public async Task<CartResponse> UpdateItemAsync(Guid userId, Guid productId, UpdateItemRequest request)
    {
        if (request.Cantidad <= 0)
            throw new ValidationException("CRT-004", "La cantidad debe ser mayor a cero.");

        var product = await GetProductAsync(productId);

        if (product == null)
            throw new NotFoundException("CRT-002", $"El producto '{productId}' no fue encontrado.");

        if (product.Stock < request.Cantidad)
            throw new BusinessRuleException("CRT-003", $"Stock insuficiente. Disponible: {product.Stock}, solicitado: {request.Cantidad}.");

        using var conn = CreateConnection();

        var itemExists = await conn.QueryFirstOrDefaultAsync<int>(
            "SELECT COUNT(1) FROM cart_items WHERE usuario_id = @userId AND producto_id = @productId",
            new { userId = userId.ToString(), productId = productId.ToString() });

        if (itemExists == 0)
            throw new NotFoundException("CRT-001", $"No existe un carrito activo para el usuario '{userId}'.");

        var now = DateTime.UtcNow.ToString("o");

        await conn.ExecuteAsync(
            "UPDATE cart_items SET cantidad = @cantidad WHERE usuario_id = @userId AND producto_id = @productId",
            new { userId = userId.ToString(), productId = productId.ToString(), cantidad = request.Cantidad });

        await conn.ExecuteAsync(
            "UPDATE carts SET fecha_actualizacion = @now WHERE usuario_id = @userId",
            new { userId = userId.ToString(), now });

        return await GetCartAsync(userId);
    }

    public async Task RemoveItemAsync(Guid userId, Guid productId)
    {
        using var conn = CreateConnection();

        var itemExists = await conn.QueryFirstOrDefaultAsync<int>(
            "SELECT COUNT(1) FROM cart_items WHERE usuario_id = @userId AND producto_id = @productId",
            new { userId = userId.ToString(), productId = productId.ToString() });

        if (itemExists == 0)
            throw new NotFoundException("CRT-001", $"No existe un carrito activo para el usuario '{userId}'.");

        var now = DateTime.UtcNow.ToString("o");

        await conn.ExecuteAsync(
            "DELETE FROM cart_items WHERE usuario_id = @userId AND producto_id = @productId",
            new { userId = userId.ToString(), productId = productId.ToString() });

        await conn.ExecuteAsync(
            "UPDATE carts SET fecha_actualizacion = @now WHERE usuario_id = @userId",
            new { userId = userId.ToString(), now });
    }

    public async Task ClearCartAsync(Guid userId)
    {
        using var conn = CreateConnection();

        var cartExists = await conn.QueryFirstOrDefaultAsync<int>(
            "SELECT COUNT(1) FROM carts WHERE usuario_id = @userId",
            new { userId = userId.ToString() });

        if (cartExists == 0)
            throw new NotFoundException("CRT-001", $"No existe un carrito activo para el usuario '{userId}'.");

        await conn.ExecuteAsync(
            "DELETE FROM cart_items WHERE usuario_id = @userId",
            new { userId = userId.ToString() });

        await conn.ExecuteAsync(
            "DELETE FROM carts WHERE usuario_id = @userId",
            new { userId = userId.ToString() });
    }

    private async Task<ProductDto?> GetProductAsync(Guid productId)
    {
        var client = httpClientFactory.CreateClient("ProductsApi");
        var response = await client.GetAsync($"/api/products/{productId}");

        if (!response.IsSuccessStatusCode)
            return null;

        return await response.Content.ReadFromJsonAsync<ProductDto>();
    }
}

public record ProductDto(Guid Id, string Nombre, int Stock, decimal Precio);