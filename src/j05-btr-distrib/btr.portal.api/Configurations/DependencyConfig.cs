using System;
using btr.application.ReportingContext.EntityAnalyticsAgg.Services;
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

            var serviceProvider = services.BuildServiceProvider();

            // Eager-init: registrars run in the bootstrap constructor and must execute before any API call.
            serviceProvider.GetRequiredService<EntityAnalyticsRegistryBootstrap>();

            return serviceProvider;
        }
    }
}
