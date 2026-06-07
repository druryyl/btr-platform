using System;
using System.Web;
using System.Web.Http;
using btr.infrastructure.Helpers;
using btr.portal.api.Configurations;
using btr.portal.api.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NLog;

namespace btr.portal.api
{
    public class WebApiApplication : HttpApplication
    {
        protected void Application_Start()
        {
            LogManager.Setup().LoadConfigurationFromFile("NLog.config");
            LogManager.GetCurrentClassLogger().Info("btr.portal.api starting");

            var configuration = new ConfigurationBuilder()
                .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{Environment.MachineName}.json", optional: true)
                .Build();

            var serviceProvider = DependencyConfig.Configure(configuration);

            ConnStringHelper.Initialize(serviceProvider.GetRequiredService<ConnectionStringFactory>());

            GlobalConfiguration.Configuration.DependencyResolver =
                new ServiceProviderDependencyResolver(serviceProvider);

            GlobalConfiguration.Configure(config => WebApiConfig.Register(config, configuration));
        }
    }
}
