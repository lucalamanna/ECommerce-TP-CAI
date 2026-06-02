using Orders.API.Data;
using Orders.API.ExceptionHandlers;
using Orders.API.HealthChecks;
using Orders.API.Services;
using System.Reflection;

namespace Orders.API.Extensions
{
    public static class ServicesExtensions
    {
        public static void AddAppServices(this IServiceCollection services)
        {
            services.AddSingleton<DatabaseInitializer>();
            services.AddScoped<OrderRepository>();
            services.AddScoped<OrderService>();

            services.AddHttpClient("ProductsApi", client =>
            {
                client.BaseAddress = new Uri("http://localhost:5100");
            });

            services.AddHttpClient("UsersApi", client =>
            {
                client.BaseAddress = new Uri("http://localhost:5200");
            });

            services.AddExceptionHandler<NotFoundExceptionHandler>();
            services.AddExceptionHandler<ValidationExceptionHandler>();
            services.AddExceptionHandler<BusinessRuleExceptionHandler>();
            services.AddExceptionHandler<BadHttpRequestExceptionHandler>();
            services.AddExceptionHandler<GlobalExceptionHandler>();
            services.AddProblemDetails();

            services.AddEndpointsApiExplorer();
            
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
                {
                    Title = "Orders API",
                    Version = "v1",
                    Description = "API de gestión de órdenes del eCommerce."
                });
                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                c.IncludeXmlComments(xmlPath);
            });

            services.AddHealthChecks()
                .AddCheck<SqliteHealthCheck>("sqlite-db", tags: ["database"])
                .AddCheck<ApiStatusCheck>("api-status", tags: ["api"]);

            services.AddHealthChecksUI(setup =>
            {
                setup.SetEvaluationTimeInSeconds(600);
                setup.AddHealthCheckEndpoint("Orders.API", "/health");
            }).AddInMemoryStorage();
        }
    }
}
