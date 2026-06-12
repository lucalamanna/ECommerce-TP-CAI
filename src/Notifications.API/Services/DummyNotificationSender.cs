namespace Notifications.API.Services;

public class DummyNotificationSender(ILogger<DummyNotificationSender> logger) : INotificationSender
{
    private readonly ILogger<DummyNotificationSender> _logger = logger;

    public Task SendAsync(string tipo, Guid usuarioId, string mensaje)
    {
        if (tipo == "Email")
        {
            _logger.LogInformation(
                "[SIMULACION EMAIL] Para: {UsuarioId} | Asunto: Notificación eCommerce | Mensaje: {Mensaje}", usuarioId, mensaje);
        }
        else if (tipo == "Push")
        {
            _logger.LogInformation("[SIMULACION PUSH] Para: {UsuarioId} | Mensaje: {Mensaje}", usuarioId, mensaje);
        }
        else if (tipo == "SMS")
        {
            _logger.LogInformation("[SIMULACION SMS] Para: {UsuarioId} | Mensaje: {Mensaje}", usuarioId, mensaje);
        }

        return Task.CompletedTask;
    }
}