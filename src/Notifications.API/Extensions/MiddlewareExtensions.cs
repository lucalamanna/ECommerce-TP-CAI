using Notifications.API.Middleware;
using Serilog;
using Serilog.Events;

namespace Notifications.API.Extensions;

public static class MiddlewareExtensions
{
    public static void UseAppMiddleware(this WebApplication app)
    {
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
        app.UseSwagger();
        app.UseSwaggerUI();
    }
}