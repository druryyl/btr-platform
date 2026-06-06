using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace btr.portal.api.Configurations
{
    public static class DependencyConfig
    {
        public static IServiceProvider Configure(IConfiguration configuration)
        {
            var services = new ServiceCollection();

            services.AddSingleton(configuration);
            services.AddApplicationPortal(configuration);
            services.AddInfrastructurePortal(configuration);
            services.AddPortalPresentation(configuration);

            return services.BuildServiceProvider();
        }
    }
}
