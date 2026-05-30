using Orders.API.DTOs;
using Orders.API.Services;
using static Orders.API.DTOs.APIErrorResponse;


namespace Orders.API.Controllers
{
    public static class OrdersEndpoints
    {
        public static void MapOrdersEndpoints(this WebApplication app)
        {
            
            app.MapGet("/api/orders", async (OrderService service, Guid? usuarioId, Guid? productoId) =>
            {
                var orders = await service.GetAllAsync(usuarioId, productoId);
                return Results.Ok(orders);
            })
            .WithTags("Orders")
            .WithSummary("Listar órdenes")
            .WithDescription("Devuelve todas las órdenes. Filtro opcional: ?usuarioId=, ?productoId=")
            .Produces<IEnumerable<OrderResponse>>(200)
            .Produces<ErrorResponse>(500);


            app.MapGet("/api/orders/{id}", async (OrderService service, Guid id) =>
            {
                var order = await service.GetByIdAsync(id);
                return Results.Ok(order);
            })
              .WithTags("Orders")
              .WithSummary("Obtener orden por ID")
              .WithDescription("Devuelve el detalle de una orden. ORD-001 (404) si no existe.")
              .Produces<OrderResponse>(200)
              .Produces<ErrorResponse>(404)
              .Produces<ErrorResponse>(500);


            app.MapPost("/api/orders", async (OrderService service, CreateOrderRequest request, HttpContext http) =>
            {
                var correlationId = http.Items["CorrelationId"]?.ToString();
                var order = await service.CreateAsync(request, correlationId);
                return Results.Created($"/api/orders/{order.Id}", order);
            })
                .WithTags("Orders")
                .WithSummary("Crear orden")
                .WithDescription("Crea una orden en estado Pendiente. " +
                    "ORD-002 (400) datos inválidos, " +
                    "ORD-003 (404) usuario no encontrado, " +
                    "ORD-004 (404) producto no encontrado, " +
                    "ORD-005 (422) stock insuficiente.")
                  .Produces<OrderResponse>(201)
                  .Produces<ErrorResponse>(400)
                  .Produces<ErrorResponse>(404)
                  .Produces<ErrorResponse>(422)
                  .Produces<ErrorResponse>(500);

            app.MapPut("/api/orders/{id}/status", async (OrderService service, Guid id, UpdateOrderStatusRequest request) =>
            {
                var updated = await service.UpdateStatusAsync(id, request);
                return Results.Ok(updated);
            })
             .WithTags("Orders")
             .WithSummary("Actualizar estado de orden")
             .WithDescription("Actualiza el estado. " +
                "ORD-001 (404) orden no encontrada, " +
                "ORD-006 (409) transición de estado inválida.")
              .Produces<UpdateOrderStatusResponse>(200)
              .Produces<ErrorResponse>(404)
               .Produces<ErrorResponse>(409)
               .Produces<ErrorResponse>(500);

            app.MapDelete("/api/orders/{id}", async (OrderService service, Guid id) =>
            {
                await service.DeleteAsync(id);
                return Results.NoContent();
            })
            .WithTags("Orders")
            .WithSummary("Eliminar orden")
            .WithDescription("Elimina una orden en estado Cancelada. " +
                "ORD-001 (404) orden no encontrada, " +
                "ORD-008 (409) la orden no está cancelada.")
            .Produces(204)
            .Produces<ErrorResponse>(404)
            .Produces<ErrorResponse>(409)
            .Produces<ErrorResponse>(500);
        }
    }
}
