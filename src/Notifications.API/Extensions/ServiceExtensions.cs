using Microsoft.OpenApi.Models;
using Notifications.API.Data;
using Notifications.API.ExceptionHandlers;
using Notifications.API.HealthChecks;
using Notifications.API.Services;
using System.Reflection;

namespace Notifications.API.Extensions;

public static class ServiceExtensions
{
    public static IServiceCollection AddAppServices(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();

        services.AddExceptionHandler<NotFoundExceptionHandler>();
        services.AddExceptionHandler<BusinessRuleExceptionHandler>();
        services.AddExceptionHandler<ValidationExceptionHandler>();
        services.AddExceptionHandler<GlobalExceptionHandler>();
        services.AddProblemDetails();

        services.AddSingleton<DatabaseInitializer>();
        services.AddScoped<NotificationRepository>();
        services.AddScoped<NotificationService>();

        services.AddHttpClient("UsersApi", client =>
        {
            client.BaseAddress = new Uri("http://localhost:5200");
        });

        services.AddHealthChecks()
            .AddCheck<SqliteHealthCheck>("sqlite-db", tags: ["database"])
            .AddCheck<ApiStatusCheck>("api-status", tags: ["api"]);

        services.AddHealthChecksUI(setup =>
        {
            setup.SetEvaluationTimeInSeconds(600);
            setup.AddHealthCheckEndpoint("Notifications.API", "/health");
        }).AddInMemoryStorage();

        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "Notifications API",
                Version = "v1",
                Description = "API de gestión de notificaciones del eCommerce."
            });

            var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            c.IncludeXmlComments(xmlPath);
        });

        return services;
    }
}