using Notifications.API.Data;
using Notifications.API.DTOs;
using Notifications.API.Exceptions;
using Notifications.API.Models;

namespace Notifications.API.Services
{
    public class NotificationService( NotificationRepository repository, IHttpClientFactory httpClientFactory, ILogger<NotificationService> logger)
    {
        private static readonly string[] TiposValidos = ["Email", "Push", "SMS"];

        public async Task<NotificationResponse> SendAsync(SendNotificationRequest request, string? correlationId)
        {
            logger.LogInformation("Enviando notificación. UsuarioId: {UsuarioId}, Tipo: {Tipo}",
                request.UsuarioId, request.Tipo);

            ValidarRequest(request);

            var usersClient = httpClientFactory.CreateClient("UsersApi");
            var userResponse = await usersClient.GetAsync($"/api/users?id={request.UsuarioId}");

            if (!userResponse.IsSuccessStatusCode)
            {
                logger.LogWarning("Usuario no encontrado. ErrorCode: NTF-001, UsuarioId: {UsuarioId}",
                    request.UsuarioId);
                throw new NotFoundException("NTF-001", "Usuario no encontrado.");
            }

            var notification = new Notification
            {
                Id = Guid.NewGuid(),
                UsuarioId = request.UsuarioId,
                Mensaje = request.Mensaje,
                Tipo = request.Tipo,
                Estado = "Enviada"
            };

            var created = await repository.CreateAsync(notification);

            logger.LogInformation("Notificación enviada. Id: {Id}, UsuarioId: {UsuarioId}",
                created.Id, created.UsuarioId);

            return MapToResponse(created);
        }

        public async Task<IEnumerable<NotificationResponse>> GetByUserIdAsync(Guid usuarioId)
        {
            logger.LogInformation("Obteniendo notificaciones. UsuarioId: {UsuarioId}", usuarioId);

            var notifications = await repository.GetByUserIdAsync(usuarioId);

            if (!notifications.Any())
            {
                logger.LogWarning("No se encontraron notificaciones. ErrorCode: NTF-003, UsuarioId: {UsuarioId}",
                    usuarioId);
                throw new NotFoundException("NTF-003", "No se encontraron notificaciones para el usuario.");
            }

            logger.LogInformation("Notificaciones obtenidas. Cantidad: {Cantidad}", notifications.Count());

            return notifications.Select(MapToResponse);
        }

        private static void ValidarRequest(SendNotificationRequest request)
        {
            var errores = new List<string>();

            if (request.UsuarioId == Guid.Empty)
                errores.Add("El campo 'UsuarioId' es requerido.");

            if (string.IsNullOrWhiteSpace(request.Mensaje))
                errores.Add("El campo 'Mensaje' es requerido.");

            if (string.IsNullOrWhiteSpace(request.Tipo) || !TiposValidos.Contains(request.Tipo))
                errores.Add($"El campo 'Tipo' debe ser uno de: {string.Join(", ", TiposValidos)}.");

            if (errores.Count > 0)
                throw new ValidationException("NTF-002", string.Join("; ", errores));
        }

        private static NotificationResponse MapToResponse(Notification notification) => new()
        {
            Id = notification.Id,
            UsuarioId = notification.UsuarioId,
            Mensaje = notification.Mensaje,
            Tipo = notification.Tipo,
            Estado = notification.Estado,
            FechaEnvio = notification.FechaEnvio
        };
    }

}
