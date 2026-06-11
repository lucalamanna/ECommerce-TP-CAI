using Notifications.API.Data;
using Notifications.API.Services;
using System.Reflection;

namespace Notifications.API.Extensions;

public static class ServicesExtensions
{
    public static void AddAppServices(this IServiceCollection services)
    {
        services.AddSingleton<DatabaseInitializer>();
        services.AddScoped<NotificationRepository>();
        services.AddScoped<NotificationService>();

        services.AddHttpClient("UsersApi", client =>
        {
            client.BaseAddress = new Uri("http://localhost:5200");
        });

        services.AddEndpointsApiExplorer();

        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
            {
                Title = "Notifications API",
                Version = "v1",
                Description = "API de gestión de notificaciones del eCommerce."
            });

            var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            c.IncludeXmlComments(xmlPath);
        });
    }
}
