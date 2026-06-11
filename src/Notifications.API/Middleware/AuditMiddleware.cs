namespace Notifications.API.Middleware;

public class AuditMiddleware(RequestDelegate next, ILogger<AuditMiddleware> logger)
{
    private static readonly string[] AuditMethods = ["POST", "PUT", "DELETE"];

    public async Task InvokeAsync(HttpContext context)
    {
        if (!AuditMethods.Contains(context.Request.Method))
        {
            await next(context);
            return;
        }

        var correlationId = context.Items["X-Correlation-Id"]?.ToString();
        var path = context.Request.Path;
        var method = context.Request.Method;

        context.Request.EnableBuffering();
        var body = await new StreamReader(context.Request.Body).ReadToEndAsync();
        context.Request.Body.Position = 0;

        logger.LogInformation(
            "Audit Request. CorrelationId: {CorrelationId}, Method: {Method}, Path: {Path}, Body: {Body}",
            correlationId, method, path, body);

        var originalBody = context.Response.Body;
        using var memStream = new MemoryStream();
        context.Response.Body = memStream;

        await next(context);

        memStream.Position = 0;
        var responseBody = await new StreamReader(memStream).ReadToEndAsync();
        memStream.Position = 0;
        await memStream.CopyToAsync(originalBody);
        context.Response.Body = originalBody;

        logger.LogInformation(
            "Audit Response. CorrelationId: {CorrelationId}, StatusCode: {StatusCode}, Body: {Body}",
            correlationId, context.Response.StatusCode, responseBody);
    }
}