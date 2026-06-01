using Cart.API.DTOs;
using Cart.API.Services;

namespace Cart.API.Controllers;

public static class CartEndpoints
{
    public static void MapCartEndpoints(this WebApplication app)
    {
        app.MapGet("/api/cart/{userId}", async (CartService service, Guid userId) =>
        {
            var cart = await service.GetCartAsync(userId);
            return Results.Ok(cart);
        })
        .WithTags("Cart")
        .WithSummary("Obtener carrito")
        .WithDescription("Devuelve el carrito activo del usuario. Devuelve 404 si no tiene carrito.")
        .Produces<CartResponse>(200)
        .Produces<ErrorResponse>(404)
        .Produces<ErrorResponse>(500);

        app.MapPost("/api/cart/{userId}/items", async (CartService service, Guid userId, AddItemRequest request) =>
        {
            var cart = await service.AddItemAsync(userId, request);
            return Results.Ok(cart);
        })
        .WithTags("Cart")
        .WithSummary("Agregar item al carrito")
        .WithDescription("Agrega un producto al carrito del usuario. Si el producto ya existe suma la cantidad.")
        .Produces<CartResponse>(200)
        .Produces<ErrorResponse>(400)
        .Produces<ErrorResponse>(404)
        .Produces<ErrorResponse>(422)
        .Produces<ErrorResponse>(500);

        app.MapPut("/api/cart/{userId}/items/{productId}", async (CartService service, Guid userId, Guid productId, UpdateItemRequest request) =>
        {
            var cart = await service.UpdateItemAsync(userId, productId, request);
            return Results.Ok(cart);
        })
        .WithTags("Cart")
        .WithSummary("Actualizar cantidad de item")
        .WithDescription("Actualiza la cantidad de un producto en el carrito del usuario.")
        .Produces<CartResponse>(200)
        .Produces<ErrorResponse>(400)
        .Produces<ErrorResponse>(404)
        .Produces<ErrorResponse>(422)
        .Produces<ErrorResponse>(500);

        app.MapDelete("/api/cart/{userId}/items/{productId}", async (CartService service, Guid userId, Guid productId) =>
        {
            await service.RemoveItemAsync(userId, productId);
            return Results.NoContent();
        })
        .WithTags("Cart")
        .WithSummary("Quitar item del carrito")
        .WithDescription("Elimina un producto específico del carrito del usuario.")
        .Produces(204)
        .Produces<ErrorResponse>(404)
        .Produces<ErrorResponse>(500);

        app.MapDelete("/api/cart/{userId}", async (CartService service, Guid userId) =>
        {
            await service.ClearCartAsync(userId);
            return Results.NoContent();
        })
        .WithTags("Cart")
        .WithSummary("Vaciar carrito")
        .WithDescription("Elimina todos los productos del carrito del usuario.")
        .Produces(204)
        .Produces<ErrorResponse>(404)
        .Produces<ErrorResponse>(500);
    }
}