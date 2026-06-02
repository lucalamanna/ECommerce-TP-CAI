using Cart.API.Extensions;
using Cart.API.Middleware;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Serilog;
using Serilog.Events;

namespace Cart.API.Extensions;

public static class MiddlewareExtensions
{
    public static WebApplication UseAppMiddleware(this WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseMiddleware<CorrelationIdMiddleware>();
        app.UseMiddleware<AuditMiddleware>();

        app.UseSerilogRequestLogging(options =>
        {
            options.GetLevel = (httpContext, _, ex) =>
                (ex != null) ? LogEventLevel.Error :
                httpContext.Request.Path.StartsWithSegments("/health")
                ? LogEventLevel.Verbose : LogEventLevel.Information;
        });

        app.UseExceptionHandler();

        app.MapHealthChecks("/health", new HealthCheckOptions
        {
            ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
        });
        app.MapHealthChecks("/health/ready", new HealthCheckOptions
        {
            ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
        });
        app.MapHealthChecks("/health/live", new HealthCheckOptions
        {
            ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
        });
        app.MapHealthChecksUI(setup => setup.UIPath = "/health-ui");

        app.MapAppEndpoints();

        return app;
    }
}