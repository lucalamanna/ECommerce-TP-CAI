using Notifications.API.Controllers;

namespace Notifications.API.Extensions
{
    public static class EndpointExtensions
    {
        public static void MapAppEndpoints(this WebApplication app)
        {
            app.MapNotificationsEndpoints();
        }
    }
}
