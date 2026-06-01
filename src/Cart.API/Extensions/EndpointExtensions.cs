using Cart.API.Controllers;

namespace Cart.API.Extensions;

public static class EndpointExtensions
{
    public static void MapAppEndpoints(this WebApplication app)
    {
        app.MapCartEndpoints();
    }
}