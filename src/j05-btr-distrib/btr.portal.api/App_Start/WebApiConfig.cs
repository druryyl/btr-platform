using System.Linq;
using System.Web.Http;
using System.Web.Http.Cors;
using System.Web.Http.Dispatcher;
using btr.portal.api.Filters;
using Microsoft.Extensions.Configuration;

namespace btr.portal.api
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config, IConfiguration configuration)
        {
            config.Services.Replace(
                typeof(IHttpControllerActivator),
                new Infrastructure.ServiceProviderControllerActivator());

            config.MapHttpAttributeRoutes();
            config.Filters.Add(new GlobalExceptionFilter());
            config.Filters.Add(new JwtAuthenticationFilter());

            var corsOrigins = configuration
                .GetSection("Cors:AllowedOrigins")
                .GetChildren()
                .Select(x => x.Value)
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .ToArray();

            if (corsOrigins.Length == 0)
                corsOrigins = new[] { "http://localhost:5173" };

            var corsAttribute = new EnableCorsAttribute(
                string.Join(",", corsOrigins),
                "*",
                "*");
            config.EnableCors(corsAttribute);
        }
    }
}
