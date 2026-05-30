using Orders.API.DTOs;
using Orders.API.Services;


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
            .WithDescription("Devuelve todas las órdenes. Filtro opcional por UsuarioID o ProductoID")
            .Produces<IEnumerable<OrderResponse>>(200)
            .Produces<ErrorResponse>(500);


            app.MapGet("/api/orders/{id}", async (OrderService service, Guid id) =>
            {
                var order = await service.GetByIdAsync(id);
                return Results.Ok(order);
            })
              .WithTags("Orders")
              .WithSummary("Obtener orden por ID")
              .WithDescription("Devuelve el detalle de una orden.")
              .Produces<OrderResponse>(200)
              .Produces<ErrorResponse>(404)
              .Produces<ErrorResponse>(500);


            app.MapPost("/api/orders", async (OrderService service, CreateOrderRequest request, HttpContext http) =>
            {
                var correlationId = http.Items["X-Correlation-Id"]?.ToString();
                var order = await service.CreateAsync(request, correlationId);
                return Results.Created($"/api/orders/{order.Id}", order);
            })
                .WithTags("Orders")
                .WithSummary("Crear orden")
                .WithDescription("Crea una orden en estado Pendiente. ")
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
             .WithDescription("Actualiza el estado de una orden existente. ")
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
            .WithDescription("Elimina una orden en estado Cancelada. ")
            .Produces(204)
            .Produces<ErrorResponse>(404)
            .Produces<ErrorResponse>(409)
            .Produces<ErrorResponse>(500);
        }
    }
}
