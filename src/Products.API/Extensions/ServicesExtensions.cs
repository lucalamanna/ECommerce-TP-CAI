using Microsoft.OpenApi.Models;
using Products.API.Data;
using Products.API.ExceptionHandlers;
using Products.API.HealthChecks;
using Products.API.Services;
using System.Reflection;

namespace Products.API.Extensions;

public static class ServicesExtensions
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
        services.AddScoped<ProductService>();

        services.AddHttpClient("OrdersApi", client =>
        {
            client.BaseAddress = new Uri("https://localhost:5300");
        });

        services.AddHealthChecks()
            .AddCheck<SqliteHealthCheck>("sqlite-db", tags: ["database"])
            .AddCheck<ApiStatusCheck>("api-status", tags: ["api"]);

        services.AddHealthChecksUI(setup =>
        {
            setup.SetEvaluationTimeInSeconds(600);
            setup.AddHealthCheckEndpoint("Products.API", "/health");
        }).AddInMemoryStorage();

        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "Products API",
                Version = "v1",
                Description = "API de gestión de productos del eCommerce."
            });

            var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            c.IncludeXmlComments(xmlPath);
        });

        return services;
    }
}