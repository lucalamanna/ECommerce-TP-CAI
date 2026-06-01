using Cart.API.Data;
using Cart.API.ExceptionHandlers;
using Cart.API.HealthChecks;
using Cart.API.Services;
using Microsoft.OpenApi.Models;
using System.Reflection;

namespace Cart.API.Extensions;

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
        services.AddScoped<CartService>();

        services.AddHttpClient("ProductsApi", client =>
        {
            client.BaseAddress = new Uri("http://localhost:5100");
        });

        services.AddHealthChecks()
            .AddCheck<SqliteHealthCheck>("sqlite-db", tags: ["database"])
            .AddCheck<ApiStatusCheck>("api-status", tags: ["api"]);

        services.AddHealthChecksUI(setup =>
        {
            setup.SetEvaluationTimeInSeconds(600);
            setup.AddHealthCheckEndpoint("Cart.API", "/health");
        }).AddInMemoryStorage();

        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "Cart API",
                Version = "v1",
                Description = "API de gestión del carrito de compras del eCommerce."
            });

            var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            c.IncludeXmlComments(xmlPath);
        });

        return services;
    }
}