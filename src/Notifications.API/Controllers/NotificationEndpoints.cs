using Notifications.API.DTOs;
using Notifications.API.Services;
using Microsoft.OpenApi.Any;

namespace Notifications.API.Controllers;

public static class NotificationEndpoints
{
    private static readonly OpenApiObject EjemploNotificacion = new()
    {
        ["id"] = new OpenApiString("11112222-3333-4444-5555-666677778888"),
        ["usuarioId"] = new OpenApiString("a1b2c3d4-0000-0000-0000-111122223333"),
        ["mensaje"] = new OpenApiString("Su orden #f1e2d3c4 fue confirmada."),
        ["tipo"] = new OpenApiString("Email"),
        ["estado"] = new OpenApiString("Enviada"),
        ["fechaEnvio"] = new OpenApiString("2024-03-10T12:01:00Z")
    };

    private static OpenApiObject ErrorNTF(string type, string title, int status, string detail, string instance, string code, string message) => new()
    {
        ["type"] = new OpenApiString(type),
        ["title"] = new OpenApiString(title),
        ["status"] = new OpenApiInteger(status),
        ["detail"] = new OpenApiString(detail),
        ["instance"] = new OpenApiString(instance),
        ["errorCode"] = new OpenApiString(code),
        ["errorMessage"] = new OpenApiString(message)
    };

    public static void MapNotificationsEndpoints(this WebApplication app)
    {
        // POST /api/notifications/send
        app.MapPost("/api/notifications/send", async (HttpContext http, NotificationService service, SendNotificationRequest request) =>
        {
            var correlationId = http.Items["X-Correlation-Id"]?.ToString();
            var notification = await service.SendAsync(request, correlationId);
            return Results.Created($"/api/notifications/{notification.Id}", notification);
        })
        .WithTags("Notifications")
        .WithSummary("Enviar notificación")
        .WithDescription("Registra y simula el envío de una notificación.")
        .Produces<NotificationResponse>(201)
        .Produces<ErrorResponse>(400)
        .Produces<ErrorResponse>(404)
        .Produces<ErrorResponse>(500)
        .WithOpenApi(op =>
        {
            op.Responses["201"].Description = "Notificación enviada exitosamente";
            op.Responses["201"].Content["application/json"].Example = EjemploNotificacion;

            op.Responses["400"].Description = "Datos inválidos (NTF-002)";
            op.Responses["400"].Content["application/json"].Example = ErrorNTF(
                "https://tools.ietf.org/html/rfc7231#section-6.5.1",
                "Bad Request", 400,
                "Los datos enviados son inválidos.",
                "/api/notifications/send", "NTF-002", "Los datos de la notificación son inválidos.");

            op.Responses["404"].Description = "Usuario no encontrado (NTF-001)";
            op.Responses["404"].Content["application/json"].Example = ErrorNTF(
                "https://tools.ietf.org/html/rfc7231#section-6.5.4",
                "Not Found", 404,
                "El recurso solicitado no fue encontrado.",
                "/api/notifications/send", "NTF-001", "El usuario destinatario no fue encontrado.");

            op.Responses["500"].Description = "Error interno (NTF-004)";
            op.Responses["500"].Content["application/json"].Example = ErrorNTF(
                "https://tools.ietf.org/html/rfc7231#section-6.6.1",
                "Internal Server Error", 500,
                "Ocurrió un error interno.",
                "/api/notifications/send", "NTF-004", "Error interno al procesar la notificación.");
            return op;
        });

        // GET /api/notifications/{userId}
        app.MapGet("/api/notifications/{userId}", async (NotificationService service, Guid userId) =>
        {
            var notifications = await service.GetByUserIdAsync(userId);
            return Results.Ok(notifications);
        })
        .WithTags("Notifications")
        .WithSummary("Listar notificaciones de un usuario")
        .WithDescription("Retorna todas las notificaciones registradas para un usuario.")
        .Produces<IEnumerable<NotificationResponse>>(200)
        .Produces<ErrorResponse>(404)
        .Produces<ErrorResponse>(500)
        .WithOpenApi(op =>
        {
            op.Responses["200"].Description = "Lista de notificaciones del usuario";
            op.Responses["200"].Content["application/json"].Example = new OpenApiArray
            {
                EjemploNotificacion
            };

            op.Responses["404"].Description = "No se encontraron notificaciones (NTF-003)";
            op.Responses["404"].Content["application/json"].Example = ErrorNTF(
                "https://tools.ietf.org/html/rfc7231#section-6.5.4",
                "Not Found", 404,
                "El recurso solicitado no fue encontrado.",
                "/api/notifications/a1b2c3d4-0000-0000-0000-111122223333", "NTF-003", "No se encontraron notificaciones para el usuario.");

            op.Responses["500"].Description = "Error interno (NTF-004)";
            op.Responses["500"].Content["application/json"].Example = ErrorNTF(
                "https://tools.ietf.org/html/rfc7231#section-6.6.1",
                "Internal Server Error", 500,
                "Ocurrió un error interno.",
                "/api/notifications/a1b2c3d4-0000-0000-0000-111122223333", "NTF-004", "Error interno al procesar la notificación.");
            return op;
        });
    }
}