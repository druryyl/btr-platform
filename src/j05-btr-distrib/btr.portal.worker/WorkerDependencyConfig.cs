using System;
using btr.portal.api.Configurations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace btr.portal.worker
{
    public static class WorkerDependencyConfig
    {
        public static IServiceProvider Configure(IConfiguration configuration)
        {
            var services = new ServiceCollection();

            services.AddSingleton(configuration);
            services.AddApplicationPortal(configuration);
            services.AddInfrastructurePortal(configuration);

            return services.BuildServiceProvider();
        }
    }
}
