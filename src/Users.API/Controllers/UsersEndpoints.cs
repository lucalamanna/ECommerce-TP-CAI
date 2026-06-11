using Microsoft.OpenApi.Any;
using Users.API.DTOs;
using Users.API.Services;

namespace Users.API.Controllers;

public static class UsersEndpoints
{
    private static readonly OpenApiObject EjemploUsuarioCompleto = new()
    {
        ["id"] = new OpenApiString("a1b2c3d4-0000-0000-0000-111122223333"),
        ["nombre"] = new OpenApiString("María"),
        ["apellido"] = new OpenApiString("González"),
        ["email"] = new OpenApiString("maria@email.com"),
        ["fechaRegistro"] = new OpenApiString("2024-03-10T09:00:00Z"),
        ["activo"] = new OpenApiBoolean(true)
    };

    private static readonly OpenApiObject EjemploLoginResponse = new()
    {
        ["id"] = new OpenApiString("a1b2c3d4-0000-0000-0000-111122223333"),
        ["nombre"] = new OpenApiString("María"),
        ["apellido"] = new OpenApiString("González"),
        ["email"] = new OpenApiString("maria@email.com")
    };

    private static OpenApiObject ErrorUSR(string type, string title, int status, string detail, string instance, string code, string message) => new()
    {
        ["type"] = new OpenApiString(type),
        ["title"] = new OpenApiString(title),
        ["status"] = new OpenApiInteger(status),
        ["detail"] = new OpenApiString(detail),
        ["instance"] = new OpenApiString(instance),
        ["errorCode"] = new OpenApiString(code),
        ["errorMessage"] = new OpenApiString(message)
    };

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
        .Produces<ErrorResponse>(404)
        .Produces<ErrorResponse>(500)
        .WithOpenApi(op =>
        {
            op.Responses["200"].Description = "Lista de usuarios";
            op.Responses["200"].Content["application/json"].Example = new OpenApiArray
            {
                EjemploUsuarioCompleto
            };

            op.Responses["404"].Description = "Usuario no encontrado (USR-007)";
            op.Responses["404"].Content["application/json"].Example = ErrorUSR(
                "https://tools.ietf.org/html/rfc7231#section-6.5.4",
                "Not Found", 404,
                "El recurso solicitado no fue encontrado.",
                "/api/users?id=00000000-0000-0000-0000-000000000000", "USR-007", "Usuario con ID '00000000-0000-0000-0000-000000000000' no encontrado.");

            op.Responses["500"].Description = "Error interno (USR-006)";
            op.Responses["500"].Content["application/json"].Example = ErrorUSR(
                "https://tools.ietf.org/html/rfc7231#section-6.6.1",
                "Internal Server Error", 500,
                "Ocurrió un error inesperado.",
                "/api/users", "USR-006", "Error interno al procesar el usuario.");
            return op;
        });

        app.MapPost("/api/users/register", async (UserService service, RegisterRequest request) =>
        {
            var user = await service.RegisterAsync(request);
            return Results.Created($"/api/users/{user.Id}", user);
        })
        .WithTags("Users")
        .WithSummary("Registrar usuario")
        .WithDescription("Registra un nuevo usuario en el sistema.")
        .Produces<UserResponse>(201)
        .Produces<ErrorResponse>(400)
        .Produces<ErrorResponse>(409)
        .Produces<ErrorResponse>(500)
        .WithOpenApi(op =>
        {
            op.Responses["201"].Description = "Usuario registrado exitosamente";
            op.Responses["201"].Content["application/json"].Example = EjemploUsuarioCompleto;

            op.Responses["400"].Description = "Datos inválidos (USR-002)";
            op.Responses["400"].Content["application/json"].Example = ErrorUSR(
                "https://tools.ietf.org/html/rfc7231#section-6.5.1",
                "Bad Request", 400,
                "Los datos enviados son inválidos.",
                "/api/users/register", "USR-002", "Los datos del usuario son inválidos.");

            op.Responses["409"].Description = "Email duplicado (USR-001)";
            op.Responses["409"].Content["application/json"].Example = ErrorUSR(
                "https://tools.ietf.org/html/rfc7231#section-6.5.9",
                "Conflict", 409,
                "Ya existe un recurso con esos datos.",
                "/api/users/register", "USR-001", "El email 'maria@email.com' ya está registrado.");

            op.Responses["500"].Description = "Error interno (USR-006)";
            op.Responses["500"].Content["application/json"].Example = ErrorUSR(
                "https://tools.ietf.org/html/rfc7231#section-6.6.1",
                "Internal Server Error", 500,
                "Ocurrió un error inesperado.",
                "/api/users/register", "USR-006", "Error interno al procesar el usuario.");
            return op;
        });

        app.MapPost("/api/users/login", async (UserService service, LoginRequest request) =>
        {
            var user = await service.LoginAsync(request);
            return Results.Ok(user);
        })
        .WithTags("Users")
        .WithSummary("Autenticar usuario")
        .WithDescription("Autentica un usuario con email y contraseña. Bloquea la cuenta tras 3 intentos fallidos consecutivos.")
        .Produces<LoginResponse>(200)
        .Produces<ErrorResponse>(400)
        .Produces<ErrorResponse>(401)
        .Produces<ErrorResponse>(403)
        .Produces<ErrorResponse>(500)
        .WithOpenApi(op =>
        {
            op.Responses["200"].Description = "Login exitoso";
            op.Responses["200"].Content["application/json"].Example = EjemploLoginResponse;

            op.Responses["400"].Description = "Datos inválidos (USR-002)";
            op.Responses["400"].Content["application/json"].Example = ErrorUSR(
                "https://tools.ietf.org/html/rfc7231#section-6.5.1",
                "Bad Request", 400,
                "Los datos enviados son inválidos.",
                "/api/users/login", "USR-002", "Los datos del usuario son inválidos.");

            op.Responses["401"].Description = "Credenciales incorrectas (USR-003)";
            op.Responses["401"].Content["application/json"].Example = ErrorUSR(
                "https://tools.ietf.org/html/rfc7235#section-3.1",
                "Unauthorized", 401,
                "Las credenciales no son válidas.",
                "/api/users/login", "USR-003", "Credenciales incorrectas.");

            op.Responses["403"].Description = "Usuario bloqueado (USR-004/USR-005)";
            op.Responses["403"].Content["application/json"].Example = ErrorUSR(
                "https://tools.ietf.org/html/rfc7231#section-6.5.3",
                "Forbidden", 403,
                "El acceso está prohibido.",
                "/api/users/login", "USR-004", "Su cuenta fue bloqueada por superar el máximo de intentos fallidos. Contacte a soporte.");

            op.Responses["500"].Description = "Error interno (USR-006)";
            op.Responses["500"].Content["application/json"].Example = ErrorUSR(
                "https://tools.ietf.org/html/rfc7231#section-6.6.1",
                "Internal Server Error", 500,
                "Ocurrió un error inesperado.",
                "/api/users/login", "USR-006", "Error interno al procesar el usuario.");
            return op;
        });
    }
}