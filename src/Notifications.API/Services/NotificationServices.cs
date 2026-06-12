using Notifications.API.Data;
using Notifications.API.DTOs;
using Notifications.API.Exceptions;
using Notifications.API.Models;

namespace Notifications.API.Services;

public class NotificationService( NotificationRepository repository, IHttpClientFactory httpClientFactory, ILogger<NotificationService> logger, INotificationSender sender)
{
    private readonly NotificationRepository _repository = repository;
    private readonly IHttpClientFactory _httpClientFactory = httpClientFactory;
    private readonly ILogger<NotificationService> _logger = logger;
    private readonly INotificationSender _sender = sender;

    private static readonly string[] TiposValidos = ["Email", "Push", "SMS"];

    public async Task<NotificationResponse> SendAsync(SendNotificationRequest request, string? correlationId)
    {
        _logger.LogInformation("Enviando notificación. UsuarioId: {UsuarioId}, Tipo: {Tipo}",
            request.UsuarioId, request.Tipo);

        ValidarRequest(request);

        var usersClient = _httpClientFactory.CreateClient("UsersApi");
        if (!string.IsNullOrWhiteSpace(correlationId))
            usersClient.DefaultRequestHeaders.Add("X-Correlation-Id", correlationId);
        var userResponse = await usersClient.GetAsync($"/api/users?id={request.UsuarioId}");

        if (!userResponse.IsSuccessStatusCode)
        {
            _logger.LogWarning("Usuario no encontrado. ErrorCode: NTF-001, UsuarioId: {UsuarioId}",
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

        var created = await _repository.CreateAsync(notification);
        await _sender.SendAsync(created.Tipo, created.UsuarioId, created.Mensaje);

        _logger.LogInformation("Notificación enviada. Id: {Id}, UsuarioId: {UsuarioId}",
            created.Id, created.UsuarioId);

        return MapToResponse(created);
    }

    public async Task<IEnumerable<NotificationResponse>> GetByUserIdAsync(Guid usuarioId)
    {
        _logger.LogInformation("Obteniendo notificaciones. UsuarioId: {UsuarioId}", usuarioId);

        var notifications = await _repository.GetByUserIdAsync(usuarioId);

        var result = new List<NotificationResponse>();
        foreach (var notification in notifications)
        {
            result.Add(MapToResponse(notification));
        }

        if (result.Count == 0)
        {
            _logger.LogWarning("No se encontraron notificaciones. ErrorCode: NTF-003, UsuarioId: {UsuarioId}",
                usuarioId);
            throw new NotFoundException("NTF-003", "No se encontraron notificaciones para el usuario.");
        }

        _logger.LogInformation("Notificaciones obtenidas. Cantidad: {Cantidad}", result.Count);

        return result;
    }
    private static void ValidarRequest(SendNotificationRequest request)
    {
        var errores = new List<string>();

        if (request.UsuarioId == Guid.Empty)
            errores.Add("El campo 'UsuarioId' es requerido.");

        if (string.IsNullOrWhiteSpace(request.Mensaje))
            errores.Add("El campo 'Mensaje' es requerido.");

        var tipoValido = false;
        for (int i = 0; i < TiposValidos.Length; i++)
        {
            if (TiposValidos[i] == request.Tipo)
            {
                tipoValido = true;
                break;
            }
        }

        if (string.IsNullOrWhiteSpace(request.Tipo) || !tipoValido)
            errores.Add("El campo 'Tipo' debe ser uno de: Email, Push, SMS.");

        if (errores.Count > 0)
        {
            var mensaje = "";
            for (int i = 0; i < errores.Count; i++)
            {
                if (i > 0) mensaje += "; ";
                mensaje += errores[i];
            }
            throw new ValidationException("NTF-002", mensaje);
        }
    }

    private static NotificationResponse MapToResponse(Notification notification)
    {
        return new NotificationResponse
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
