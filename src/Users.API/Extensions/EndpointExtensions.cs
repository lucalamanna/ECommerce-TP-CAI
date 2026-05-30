using Users.API.Controllers;

namespace Users.API.Extensions;

public static class EndpointExtensions
{
    public static void MapAppEndpoints(this WebApplication app)
    {
        app.MapUsersEndpoints();
    }
}