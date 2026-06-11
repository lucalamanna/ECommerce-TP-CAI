namespace Notifications.API.Middleware;

public class CorrelationIdMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = context.Request.Headers["X-Correlation-Id"].FirstOrDefault()
            ?? Guid.NewGuid().ToString();

        context.Items["X-Correlation-Id"] = correlationId;
        context.Response.Headers["x-correlation-id"] = correlationId;

        await next(context);
    }
}