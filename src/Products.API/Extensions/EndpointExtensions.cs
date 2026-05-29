using Products.API.Controllers;

namespace Products.API.Extensions;

public static class EndpointExtensions
{
    public static void MapAppEndpoints(this WebApplication app)
    {
        // Se agregan todos los endpoints del microservicio Products
        app.MapProductsEndpoints();
    }
}