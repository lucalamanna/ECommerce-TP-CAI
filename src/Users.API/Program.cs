using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Serilog;
using Users.API;
using Users.API.Controllers;
using Users.API.ExceptionHandlers;
using Users.API.Extensions;
using Users.API.HealthChecks;
using Serilog.Events;
using Users.API.Middleware;
using Users.API.Services;

var builder = WebApplication.CreateBuilder(args);

builder.AddAppLogging();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Users API",
        Version = "v1",
        Description = "API de gestión de usuarios del eCommerce."
    });
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = System.IO.Path.Combine(AppContext.BaseDirectory, xmlFile);
    c.IncludeXmlComments(xmlPath);
});

builder.Services.AddExceptionHandler<NotFoundExceptionHandler>();
builder.Services.AddExceptionHandler<BusinessRuleExceptionHandler>();
builder.Services.AddExceptionHandler<ValidationExceptionHandler>();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

builder.Services.AddSingleton<DatabaseInitializer>();
builder.Services.AddSingleton<UserService>();

builder.Services.AddHealthChecks()
    .AddCheck<SqliteHealthCheck>("sqlite-db", tags: ["database"])
    .AddCheck<ApiStatusCheck>("api-status", tags: ["api"]);
builder.Services.AddHealthChecksUI(setup =>
{
    setup.SetEvaluationTimeInSeconds(600);
    setup.AddHealthCheckEndpoint("Users.API", "/health");
}).AddInMemoryStorage();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
    scope.ServiceProvider.GetRequiredService<DatabaseInitializer>().Initialize();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<CorrelationIdMiddleware>();

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

app.MapUsersEndpoints();

app.Run();

