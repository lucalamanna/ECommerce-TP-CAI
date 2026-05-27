using Users.API.DTOs;
using Users.API.Services;

namespace Users.API.Controllers;

public static class UsersEndpoints
{
    public static void MapUsersEndpoints(this WebApplication app)
    {
        app.MapPost("/api/users/register", async (UserService service, RegisterRequest request) =>
        {
            var user = await service.RegisterAsync(request);
            return Results.Created($"/api/users/{user.Id}", user);
        })
        .WithTags("Users");

        app.MapPost("/api/users/login", async (UserService service, LoginRequest request) =>
        {
            var user = await service.LoginAsync(request);
            return Results.Ok(user);
        })
        .WithTags("Users");
    }
}