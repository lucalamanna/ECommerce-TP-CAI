using Notifications.API.DTOs;
using Notifications.API.Services;

namespace Notifications.API.Controllers
{
    public static class NotificationEndpoints
    {
        public static void MapNotificationsEndpoints(this WebApplication app)
        {
            app.MapPost("/api/notifications/send", async ( HttpContext http, NotificationService service, SendNotificationRequest request) =>
            {
                var correlationId = http.Items["X-Correlation-Id"]?.ToString();
                var notification = await service.SendAsync(request, correlationId);
                return Results.Created($"/api/notifications/{notification.Id}", notification);
            })
            .WithTags("Notifications")
            .WithSummary("Enviar notificación")
            .WithDescription("Registra y simula el envío de una notificación.");
        }
    }
}
