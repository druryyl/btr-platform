using btr.application.SupportContext.UserAgg;
using btr.infrastructure.SupportContext.UserAgg;
using btr.portal.api.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace btr.portal.api.Configurations
{
    public static class PortalPresentationExtensions
    {
        public static IServiceCollection AddPortalPresentation(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));
            services.AddScoped<IUserDal, UserDal>();
            services.AddSingleton<IJwtTokenService, JwtTokenService>();
            services.AddTransient<Controllers.AuthController>();
            services.AddTransient<Controllers.Dashboard.OverviewDashboardController>();
            services.AddTransient<Controllers.Dashboard.SalesDashboardController>();
            services.AddTransient<Controllers.Dashboard.PiutangDashboardController>();
            services.AddTransient<Controllers.Dashboard.InventoryDashboardController>();
            services.AddTransient<Controllers.Reports.SalesReportController>();
            services.AddTransient<Controllers.Reports.InventoryReportController>();
            services.AddTransient<Controllers.Reports.PiutangReportController>();
            services.AddTransient<Controllers.Reports.PurchasingReportController>();

            return services;
        }
    }
}
