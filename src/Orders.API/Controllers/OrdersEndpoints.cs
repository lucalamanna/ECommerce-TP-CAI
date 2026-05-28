using Orders.API.DTOs;
using Orders.API.Services;

namespace Orders.API.Controllers
{
    public static class OrdersEndpoints
    {
        public static void MapOrdersEndpoints(this WebApplication app)
        {
            // GET /api/orders
            app.MapGet("/api/orders", async (OrderService service, Guid? usuarioId) =>
            {
                var orders = await service.GetAllAsync(usuarioId);
                return Results.Ok(orders);
            }).WithTags("Orders");

            // GET /api/orders/{id}
            app.MapGet("/api/orders/{id}", async (OrderService service, Guid id) =>
            {
                var order = await service.GetByIdAsync(id);
                return Results.Ok(order);
            }).WithTags("Orders");

            // POST /api/orders
            app.MapPost("/api/orders", async (OrderService service, CreateOrderRequest request) =>
            {
                // TODO: implementar cuando se resuelva IHttpClientFactory
                // var order = await service.CreateAsync(request);
                // return Results.Created($"/api/orders/{order.Id}", order);
                return Results.StatusCode(501);
                //var order = await service.CreateAsync(request);
                //return Results.Created($"/api/orders/{order.Id}", order);
            }).WithTags("Orders");

            // PUT /api/orders/{id}/status
            app.MapPut("/api/orders/{id}/status", async (OrderService service, Guid id, UpdateOrderStatusRequest request) =>
            {
                var updated = await service.UpdateStatusAsync(id, request);
                return Results.Ok(updated);
            }).WithTags("Orders");

            app.MapDelete("/api/orders/{id}", async (OrderService service, Guid id) =>
            {
                await service.DeleteAsync(id);
                return Results.NoContent();
            }).WithTags("Orders");
        }
    }
}
