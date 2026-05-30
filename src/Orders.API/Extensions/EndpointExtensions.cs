using Orders.API.Controllers;

namespace Orders.API.Extensions
   
{
    public static class EndpointExtensions
    {
        public static void MapAppEndpoints(this WebApplication app)
    {
        app.MapOrdersEndpoints();
    }
    }
}
