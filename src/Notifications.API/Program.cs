using Notifications.API.Data;
using Notifications.API.Extensions;

var builder = WebApplication.CreateBuilder(args);
builder.AddAppLogging();
builder.Services.AddAppServices();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
    scope.ServiceProvider.GetRequiredService<DatabaseInitializer>().Initialize();

app.UseAppMiddleware();

app.Run();