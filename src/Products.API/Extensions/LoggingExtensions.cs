using Serilog;
using Serilog.Events;
using Serilog.Filters;
using Serilog.Formatting.Json;  

namespace Products.API.Extensions;

public static class LoggingExtensions
{
    public static void AddAppLogging(this WebApplicationBuilder builder)
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
            .MinimumLevel.Override("Microsoft.AspNetCore.Hosting.Diagnostics", LogEventLevel.Information)
            .Enrich.FromLogContext()
            .Enrich.WithProperty("Servicio", "Products.API")

            .Filter.ByExcluding(Matching.WithProperty<string>("RequestPath",
                path => path.Contains("/swagger") || path.Contains("/health")))

            .WriteTo.Console(outputTemplate:
                "[{Timestamp:HH:mm:ss} {Level:u3}] [{Servicio}] [{CorrelationId}] {Message:lj}{NewLine}{Exception}")

            .WriteTo.File(
                formatter: new JsonFormatter(),
                path: "logs/products-.json",
                rollingInterval: RollingInterval.Day)
            .CreateLogger();

        builder.Host.UseSerilog();
    }
}