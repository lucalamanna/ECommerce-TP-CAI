using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Orders.API.Middleware;
using Serilog;
using Serilog.Events;

namespace Orders.API.Extensions
{
    public static class MiddlewareExtensions
    {
        public static void UseAppMiddleware(this WebApplication app)
        {
            app.UseMiddleware<CorrelationIdMiddleware>();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseSerilogRequestLogging(options =>
            {
                options.GetLevel = (httpContext, _, ex) =>
                    (ex != null) ? LogEventLevel.Error :
                    httpContext.Request.Path.StartsWithSegments("/health")
                        ? LogEventLevel.Verbose : LogEventLevel.Information;
            });

            app.UseMiddleware<AuditMiddleware>();
            app.UseExceptionHandler();

            app.MapHealthChecks("/health", new HealthCheckOptions { ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse });
            app.MapHealthChecks("/health/ready", new HealthCheckOptions { ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse });
            app.MapHealthChecks("/health/live", new HealthCheckOptions { ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse });
            app.MapHealthChecksUI(setup => setup.UIPath = "/health-ui");
        }
    }
}

