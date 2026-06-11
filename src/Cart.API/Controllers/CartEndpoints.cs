using Cart.API.DTOs;
using Cart.API.Services;
using Microsoft.OpenApi.Any;
using Microsoft.AspNetCore.OpenApi;

namespace Cart.API.Controllers;

public static class CartEndpoints
{
    private static readonly OpenApiObject EjemploCarrito = new()
    {
        ["usuarioId"] = new OpenApiString("a1b2c3d4-0000-0000-0000-111122223333"),
        ["items"] = new OpenApiArray
        {
            new OpenApiObject
            {
                ["productoId"] = new OpenApiString("3fa85f64-5717-4562-b3fc-2c963f66afa6"),
                ["cantidad"] = new OpenApiInteger(2)
            }
        },
        ["fechaActualizacion"] = new OpenApiString("2024-03-10T10:45:00Z")
    };

    private static OpenApiObject ErrorCRT(string type, string title, int status, string detail, string instance, string code, string message) => new()
    {
        ["type"] = new OpenApiString(type),
        ["title"] = new OpenApiString(title),
        ["status"] = new OpenApiInteger(status),
        ["detail"] = new OpenApiString(detail),
        ["instance"] = new OpenApiString(instance),
        ["errorCode"] = new OpenApiString(code),
        ["errorMessage"] = new OpenApiString(message)
    };

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
        .Produces<ErrorResponse>(500)
        .WithOpenApi(op =>
        {
            op.Responses["200"].Description = "Carrito del usuario";
            op.Responses["200"].Content["application/json"].Example = EjemploCarrito;

            op.Responses["404"].Description = "Carrito no encontrado (CRT-001)";
            op.Responses["404"].Content["application/json"].Example = ErrorCRT(
                "https://tools.ietf.org/html/rfc7231#section-6.5.4",
                "Not Found", 404,
                "El recurso solicitado no fue encontrado.",
                "/api/cart/a1b2c3d4-0000-0000-0000-111122223333", "CRT-001", "No existe un carrito activo para el usuario.");

            op.Responses["500"].Description = "Error interno (CRT-005)";
            op.Responses["500"].Content["application/json"].Example = ErrorCRT(
                "https://tools.ietf.org/html/rfc7231#section-6.6.1",
                "Internal Server Error", 500,
                "Ocurrió un error interno.",
                "/api/cart/a1b2c3d4-0000-0000-0000-111122223333", "CRT-005", "Error interno al procesar el carrito.");
            return op;
        });

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
        .Produces<ErrorResponse>(500)
        .WithOpenApi(op =>
        {
            op.Responses["200"].Description = "Item agregado exitosamente";
            op.Responses["200"].Content["application/json"].Example = EjemploCarrito;

            op.Responses["400"].Description = "Cantidad inválida (CRT-004)";
            op.Responses["400"].Content["application/json"].Example = ErrorCRT(
                "https://tools.ietf.org/html/rfc7231#section-6.5.1",
                "Bad Request", 400,
                "Los datos enviados son inválidos.",
                "/api/cart/a1b2c3d4-0000-0000-0000-111122223333/items", "CRT-004", "La cantidad debe ser mayor a cero.");

            op.Responses["404"].Description = "Producto no encontrado (CRT-002)";
            op.Responses["404"].Content["application/json"].Example = ErrorCRT(
                "https://tools.ietf.org/html/rfc7231#section-6.5.4",
                "Not Found", 404,
                "El recurso solicitado no fue encontrado.",
                "/api/cart/a1b2c3d4-0000-0000-0000-111122223333/items", "CRT-002", "El producto no fue encontrado.");

            op.Responses["422"].Description = "Stock insuficiente (CRT-003)";
            op.Responses["422"].Content["application/json"].Example = ErrorCRT(
                "https://tools.ietf.org/html/rfc4918#section-11.2",
                "Unprocessable Entity", 422,
                "No se puede procesar la solicitud.",
                "/api/cart/a1b2c3d4-0000-0000-0000-111122223333/items", "CRT-003", "Stock insuficiente. Disponible: 2, solicitado: 5.");

            op.Responses["500"].Description = "Error interno (CRT-005)";
            op.Responses["500"].Content["application/json"].Example = ErrorCRT(
                "https://tools.ietf.org/html/rfc7231#section-6.6.1",
                "Internal Server Error", 500,
                "Ocurrió un error interno.",
                "/api/cart/a1b2c3d4-0000-0000-0000-111122223333/items", "CRT-005", "Error interno al procesar el carrito.");
            return op;
        });

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
        .Produces<ErrorResponse>(500)
        .WithOpenApi(op =>
        {
            op.Responses["200"].Description = "Cantidad actualizada exitosamente";
            op.Responses["200"].Content["application/json"].Example = EjemploCarrito;

            op.Responses["400"].Description = "Cantidad inválida (CRT-004)";
            op.Responses["400"].Content["application/json"].Example = ErrorCRT(
                "https://tools.ietf.org/html/rfc7231#section-6.5.1",
                "Bad Request", 400,
                "Los datos enviados son inválidos.",
                "/api/cart/a1b2c3d4-0000-0000-0000-111122223333/items/3fa85f64-5717-4562-b3fc-2c963f66afa6", "CRT-004", "La cantidad debe ser mayor a cero.");

            op.Responses["404"].Description = "Carrito o producto no encontrado (CRT-001/CRT-002)";
            op.Responses["404"].Content["application/json"].Example = ErrorCRT(
                "https://tools.ietf.org/html/rfc7231#section-6.5.4",
                "Not Found", 404,
                "El recurso solicitado no fue encontrado.",
                "/api/cart/a1b2c3d4-0000-0000-0000-111122223333/items/3fa85f64-5717-4562-b3fc-2c963f66afa6", "CRT-001", "No existe un carrito activo para el usuario.");

            op.Responses["422"].Description = "Stock insuficiente (CRT-003)";
            op.Responses["422"].Content["application/json"].Example = ErrorCRT(
                "https://tools.ietf.org/html/rfc4918#section-11.2",
                "Unprocessable Entity", 422,
                "No se puede procesar la solicitud.",
                "/api/cart/a1b2c3d4-0000-0000-0000-111122223333/items/3fa85f64-5717-4562-b3fc-2c963f66afa6", "CRT-003", "Stock insuficiente. Disponible: 2, solicitado: 5.");

            op.Responses["500"].Description = "Error interno (CRT-005)";
            op.Responses["500"].Content["application/json"].Example = ErrorCRT(
                "https://tools.ietf.org/html/rfc7231#section-6.6.1",
                "Internal Server Error", 500,
                "Ocurrió un error interno.",
                "/api/cart/a1b2c3d4-0000-0000-0000-111122223333/items/3fa85f64-5717-4562-b3fc-2c963f66afa6", "CRT-005", "Error interno al procesar el carrito.");
            return op;
        });

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
        .Produces<ErrorResponse>(500)
        .WithOpenApi(op =>
        {
            op.Responses["204"].Description = "Item eliminado exitosamente";

            op.Responses["404"].Description = "Carrito no encontrado (CRT-001)";
            op.Responses["404"].Content["application/json"].Example = ErrorCRT(
                "https://tools.ietf.org/html/rfc7231#section-6.5.4",
                "Not Found", 404,
                "El recurso solicitado no fue encontrado.",
                "/api/cart/a1b2c3d4-0000-0000-0000-111122223333/items/3fa85f64-5717-4562-b3fc-2c963f66afa6", "CRT-001", "No existe un carrito activo para el usuario.");

            op.Responses["500"].Description = "Error interno (CRT-005)";
            op.Responses["500"].Content["application/json"].Example = ErrorCRT(
                "https://tools.ietf.org/html/rfc7231#section-6.6.1",
                "Internal Server Error", 500,
                "Ocurrió un error interno.",
                "/api/cart/a1b2c3d4-0000-0000-0000-111122223333/items/3fa85f64-5717-4562-b3fc-2c963f66afa6", "CRT-005", "Error interno al procesar el carrito.");
            return op;
        });

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
        .Produces<ErrorResponse>(500)
        .WithOpenApi(op =>
        {
            op.Responses["204"].Description = "Carrito vaciado exitosamente";

            op.Responses["404"].Description = "Carrito no encontrado (CRT-001)";
            op.Responses["404"].Content["application/json"].Example = ErrorCRT(
                "https://tools.ietf.org/html/rfc7231#section-6.5.4",
                "Not Found", 404,
                "El recurso solicitado no fue encontrado.",
                "/api/cart/a1b2c3d4-0000-0000-0000-111122223333", "CRT-001", "No existe un carrito activo para el usuario.");

            op.Responses["500"].Description = "Error interno (CRT-005)";
            op.Responses["500"].Content["application/json"].Example = ErrorCRT(
                "https://tools.ietf.org/html/rfc7231#section-6.6.1",
                "Internal Server Error", 500,
                "Ocurrió un error interno.",
                "/api/cart/a1b2c3d4-0000-0000-0000-111122223333", "CRT-005", "Error interno al procesar el carrito.");
            return op;
        });
    }
}