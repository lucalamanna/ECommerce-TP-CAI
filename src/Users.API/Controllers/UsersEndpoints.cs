using Users.API.DTOs;
using Users.API.Services;

namespace Users.API.Controllers;

public static class UsersEndpoints
{
    public static void MapUsersEndpoints(this WebApplication app)
    {
        app.MapGet("/api/users", async (UserService service, string? id, string? email, string? nombre, string? apellido) =>
        {
            var users = await service.GetAllAsync(id, email, nombre, apellido);
            return Results.Ok(users);
        })
        .WithTags("Users")
        .WithSummary("Listar usuarios")
        .WithDescription("Devuelve todos los usuarios. Se puede filtrar por id, email, nombre y/o apellido. Si se filtra por id y no existe, devuelve 404.")
        .Produces<IEnumerable<UserResponse>>(200)
        .Produces(404)
        .Produces(500);

        app.MapPost("/api/users/register", async (UserService service, RegisterRequest request) =>
        {
            var user = await service.RegisterAsync(request);
            return Results.Created($"/api/users/{user.Id}", user);
        })
        .WithTags("Users")
        .WithSummary("Registrar usuario")
        .WithDescription("Registra un nuevo usuario en el sistema.")
        .Produces<UserResponse>(201)
        .Produces(400)
        .Produces(409)
        .Produces(500);

        app.MapPost("/api/users/login", async (UserService service, LoginRequest request) =>
        {
            var user = await service.LoginAsync(request);
            return Results.Ok(user);
        })
        .WithTags("Users")
        .WithSummary("Autenticar usuario")
        .WithDescription("Autentica un usuario con email y contraseña. Bloquea la cuenta tras 3 intentos fallidos consecutivos.")
        .Produces<LoginResponse>(200)
        .Produces(400)
        .Produces(401)
        .Produces(403)
        .Produces(500);
    }
}