using Microsoft.OpenApi.Any;
using Orders.API.DTOs;
using Orders.API.Services;

namespace Orders.API.Controllers;

public static class OrdersEndpoints
{
    private static readonly OpenApiObject EjemploOrden = new()
    {
        ["id"] = new OpenApiString("ed280b20-d423-4cdf-8434-2d3f704cced9"),
        ["usuarioId"] = new OpenApiString("a6041fe0-1fee-4eb4-a205-ca972e2f47ae"),
        ["items"] = new OpenApiArray
        {
            new OpenApiObject
            {
                ["productoId"] = new OpenApiString("5eb1b5ac-bc01-4f7a-93b3-dc27732612aa"),
                ["cantidad"] = new OpenApiInteger(2),
                ["precioUnitario"] = new OpenApiDouble(2000)
            }
        },
        ["total"] = new OpenApiDouble(4000),
        ["estado"] = new OpenApiString("Pendiente"),
        ["fechaCreacion"] = new OpenApiString("2026-06-01T02:33:35")
    };

    private static readonly OpenApiObject EjemploUpdateStatus = new()
    {
        ["id"] = new OpenApiString("ed280b20-d423-4cdf-8434-2d3f704cced9"),
        ["estado"] = new OpenApiString("Confirmada"),
        ["fechaActualizacion"] = new OpenApiString("2026-06-01T03:00:00Z")
    };

    private static OpenApiObject ErrorORD(string type, string title, int status,
        string detail, string instance, string code, string message) => new()
        {
            ["type"] = new OpenApiString(type),
            ["title"] = new OpenApiString(title),
            ["status"] = new OpenApiInteger(status),
            ["detail"] = new OpenApiString(detail),
            ["instance"] = new OpenApiString(instance),
            ["errorCode"] = new OpenApiString(code),
            ["errorMessage"] = new OpenApiString(message)
        };

    public static void MapOrdersEndpoints(this WebApplication app)
    {
        // GET /api/orders
        app.MapGet("/api/orders", async (OrderService service, Guid? usuarioId, Guid? productoId) =>
        {
            var orders = await service.GetAllAsync(usuarioId, productoId);
            return Results.Ok(orders);
        })
        .WithTags("Orders")
        .WithSummary("Listar órdenes")
        .WithDescription("Retorna todas las órdenes. Se puede filtrar por usuarioId y/o productoId.")
        .Produces<IEnumerable<OrderResponse>>(200)
        .Produces<ErrorResponse>(500)
        .WithOpenApi(op =>
        {
            op.Responses["200"].Description = "Lista de órdenes";
            op.Responses["200"].Content["application/json"].Example = new OpenApiArray { EjemploOrden };
            op.Responses["500"].Description = "Error interno (ORD-007)";
            op.Responses["500"].Content["application/json"].Example = ErrorORD(
                "https://tools.ietf.org/html/rfc7231#section-6.6.1",
                "Internal Server Error", 500,
                "Ocurrió un error interno.",
                "/api/orders", "ORD-007", "Error interno al procesar la orden.");
            return op;
        });

        // GET /api/orders/{id}
        app.MapGet("/api/orders/{id}", async (OrderService service, Guid id) =>
        {
            var order = await service.GetByIdAsync(id);
            return Results.Ok(order);
        })
        .WithTags("Orders")
        .WithSummary("Obtener orden por ID")
        .WithDescription("Retorna una orden específica según su identificador único.")
        .Produces<OrderResponse>(200)
        .Produces<ErrorResponse>(404)
        .Produces<ErrorResponse>(500)
        .WithOpenApi(op =>
        {
            op.Responses["200"].Description = "Orden encontrada";
            op.Responses["200"].Content["application/json"].Example = EjemploOrden;
            op.Responses["404"].Description = "Orden no encontrada (ORD-001)";
            op.Responses["404"].Content["application/json"].Example = ErrorORD(
                "https://tools.ietf.org/html/rfc7231#section-6.5.4",
                "Not Found", 404,
                "El recurso solicitado no fue encontrado.",
                "/api/orders/f1e2d3c4-0000-0000-0000-aabbccddeeff",
                "ORD-001", "Orden no encontrada.");
            op.Responses["500"].Description = "Error interno (ORD-007)";
            op.Responses["500"].Content["application/json"].Example = ErrorORD(
                "https://tools.ietf.org/html/rfc7231#section-6.6.1",
                "Internal Server Error", 500,
                "Ocurrió un error interno.",
                "/api/orders/f1e2d3c4-0000-0000-0000-aabbccddeeff",
                "ORD-007", "Error interno al procesar la orden.");
            return op;
        });

        // POST /api/orders
        app.MapPost("/api/orders", async (HttpContext http, OrderService service, CreateOrderRequest request) =>
        {
            var correlationId = http.Items["X-Correlation-Id"]?.ToString();
            var order = await service.CreateAsync(request, correlationId);
            return Results.Created($"/api/orders/{order.Id}", order);
        })
        .WithTags("Orders")
        .WithSummary("Crear orden")
        .WithDescription("Crea una nueva orden en estado Pendiente.")
        .Produces<OrderResponse>(201)
        .Produces<ErrorResponse>(400)
        .Produces<ErrorResponse>(404)
        .Produces<ErrorResponse>(422)
        .Produces<ErrorResponse>(500)
        .WithOpenApi(op =>
        {
            op.Responses["201"].Description = "Orden creada exitosamente";
            op.Responses["201"].Content["application/json"].Example = EjemploOrden;
            op.Responses["400"].Description = "Datos inválidos (ORD-002): items vacíos o cantidad menor o igual a 0";
            op.Responses["400"].Content["application/json"].Example = ErrorORD(
                "https://tools.ietf.org/html/rfc7231#section-6.5.1",
                "Bad Request", 400,
                "Los datos enviados son inválidos.",
                "/api/orders", "ORD-002", "Los datos de la orden son inválidos.");
            op.Responses["404"].Description = "Usuario no encontrado (ORD-003) o Producto no encontrado (ORD-004)";
            op.Responses["404"].Content["application/json"].Example = ErrorORD(
                "https://tools.ietf.org/html/rfc7231#section-6.5.4",
                "Not Found", 404,
                "El recurso solicitado no fue encontrado.",
                "/api/orders", "ORD-004",
                "Producto '00000000-0000-0000-0000-000000000001' no encontrado.");
            op.Responses["409"].Description = "El usuario ya tiene una orden pendiente (ORD-009)";
            op.Responses["409"].Content["application/json"].Example = ErrorORD(
                "https://tools.ietf.org/html/rfc7231#section-6.5.9",
                "Conflict", 409,
                "Ya existe un recurso con esos datos.",
                "/api/orders", "ORD-009",
                "El usuario ya tiene una orden en estado Pendiente.");
            op.Responses["422"].Description = "Stock insuficiente (ORD-005)";
            op.Responses["422"].Content["application/json"].Example = ErrorORD(
                "https://tools.ietf.org/html/rfc4918#section-11.2",
                "Unprocessable Entity", 422,
                "No se puede procesar la solicitud.",
                "/api/orders", "ORD-005",
                "Stock insuficiente para 'Aire acondicionado'. Disponible: 10, solicitado: 11.");
            op.Responses["500"].Description = "Error interno (ORD-007)";
            op.Responses["500"].Content["application/json"].Example = ErrorORD(
                "https://tools.ietf.org/html/rfc7231#section-6.6.1",
                "Internal Server Error", 500,
                "Ocurrió un error interno.",
                "/api/orders", "ORD-007", "Error interno al procesar la orden.");
            return op;
        });

        // PUT /api/orders/{id}/status
        app.MapPut("/api/orders/{id}/status", async (OrderService service, Guid id, UpdateOrderStatusRequest request) =>
        {
            var result = await service.UpdateStatusAsync(id, request);
            return Results.Ok(result);
        })
        .WithTags("Orders")
        .WithSummary("Actualizar estado de orden")
        .WithDescription("Actualiza el estado de una orden. Transiciones válidas: Pendiente→Confirmada, Pendiente→Cancelada, Confirmada→Enviada, Confirmada→Cancelada, Enviada→Entregada.")
        .Produces<UpdateOrderStatusResponse>(200)
        .Produces<ErrorResponse>(400)
        .Produces<ErrorResponse>(404)
        .Produces<ErrorResponse>(409)        
        .Produces<ErrorResponse>(500)
        .WithOpenApi(op =>
        {
            op.Responses["200"].Description = "Estado actualizado exitosamente";
            op.Responses["200"].Content["application/json"].Example = EjemploUpdateStatus;
            op.Responses["400"].Description = "Datos inválidos (ORD-002)";
            op.Responses["400"].Content["application/json"].Example = ErrorORD(
                "https://tools.ietf.org/html/rfc7231#section-6.5.1",
                "Bad Request", 400,
                "Los datos enviados son inválidos.",
                "/api/orders/f1e2d3c4-0000-0000-0000-aabbccddeeff/status",
                "ORD-002", "Los datos de la orden son inválidos.");
            op.Responses["404"].Description = "Orden no encontrada (ORD-001)";
            op.Responses["404"].Content["application/json"].Example = ErrorORD(
                "https://tools.ietf.org/html/rfc7231#section-6.5.4",
                "Not Found", 404,
                "El recurso solicitado no fue encontrado.",
                "/api/orders/f1e2d3c4-0000-0000-0000-aabbccddeeff/status",
                "ORD-001", "Orden no encontrada.");
            op.Responses["409"].Description = "Transición de estado inválida (ORD-006)";
            op.Responses["409"].Content["application/json"].Example = ErrorORD(
                "https://tools.ietf.org/html/rfc7231#section-6.5.9",
                "Conflict", 409,
                "No se puede modificar el estado.",
                "/api/orders/f1e2d3c4-0000-0000-0000-aabbccddeeff/status",
                "ORD-006", "Una orden en estado 'Confirmada' no puede pasar a 'Pendiente'.");
            op.Responses["500"].Description = "Error interno (ORD-007)";
            op.Responses["500"].Content["application/json"].Example = ErrorORD(
                "https://tools.ietf.org/html/rfc7231#section-6.6.1",
                "Internal Server Error", 500,
                "Ocurrió un error interno.",
                "/api/orders/f1e2d3c4-0000-0000-0000-aabbccddeeff/status",
                "ORD-007", "Error interno al procesar la orden.");
            return op;
        });

        // DELETE /api/orders/{id}
        app.MapDelete("/api/orders/{id}", async (OrderService service, Guid id) =>
        {
            await service.DeleteAsync(id);
            return Results.NoContent();
        })
        .WithTags("Orders")
        .WithSummary("Eliminar orden")
        .WithDescription("Elimina una orden. Solo se pueden eliminar órdenes en estado Cancelada.")
        .Produces(204)
        .Produces<ErrorResponse>(404)
        .Produces<ErrorResponse>(409)
        .Produces<ErrorResponse>(500)
        .WithOpenApi(op =>
        {
            op.Responses["204"].Description = "Orden eliminada exitosamente";
            op.Responses["404"].Description = "Orden no encontrada (ORD-001)";
            op.Responses["404"].Content["application/json"].Example = ErrorORD(
                "https://tools.ietf.org/html/rfc7231#section-6.5.4",
                "Not Found", 404,
                "El recurso solicitado no fue encontrado.",
                "/api/orders/f1e2d3c4-0000-0000-0000-aabbccddeeff",
                "ORD-001", "Orden no encontrada.");
            op.Responses["409"].Description = "La orden no está en estado Cancelada (ORD-008)";
            op.Responses["409"].Content["application/json"].Example = ErrorORD(
                "https://tools.ietf.org/html/rfc7231#section-6.5.9",
                "Conflict", 409,
                "No se puede eliminar la orden.",
                "/api/orders/f1e2d3c4-0000-0000-0000-aabbccddeeff",
                "ORD-008", "Solo se pueden eliminar órdenes en estado 'Cancelada'.");
            op.Responses["500"].Description = "Error interno (ORD-007)";
            op.Responses["500"].Content["application/json"].Example = ErrorORD(
                "https://tools.ietf.org/html/rfc7231#section-6.6.1",
                "Internal Server Error", 500,
                "Ocurrió un error interno.",
                "/api/orders/f1e2d3c4-0000-0000-0000-aabbccddeeff",
                "ORD-007", "Error interno al procesar la orden.");
            return op;
        });
    }
}
