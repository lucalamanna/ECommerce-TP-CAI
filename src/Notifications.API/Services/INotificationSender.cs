namespace Notifications.API.Services;

public interface INotificationSender
{
    Task SendAsync(string tipo, Guid usuarioId, string mensaje);
}
