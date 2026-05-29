using Orders.API.Data;
using Orders.API.Extensions;

var builder = WebApplication.CreateBuilder(args);
builder.AddAppLogging();
builder.Services.AddAppServices();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
    scope.ServiceProvider.GetRequiredService<DatabaseInitializer>().Initialize();

app.UseAppMiddleware();
app.MapAppEndpoints();   // tu UseAppMiddleware NO llama a esto (el de Products sí), así que en Orders va acá

app.Run();


