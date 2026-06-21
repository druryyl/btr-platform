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
            services.AddTransient<Controllers.PresentationConfigController>();
            services.AddTransient<Controllers.HealthController>();
            services.AddTransient<Controllers.Admin.AdminDashboardRefreshController>();
            services.AddTransient<Controllers.Dashboard.OverviewDashboardController>();
            services.AddTransient<Controllers.Dashboard.ExecutiveDashboardController>();
            services.AddTransient<Controllers.Dashboard.AlertCenterDashboardController>();
            services.AddTransient<Controllers.Dashboard.SalesDashboardController>();
            services.AddTransient<Controllers.Dashboard.SalesForecastDashboardController>();
            services.AddTransient<Controllers.Dashboard.PiutangDashboardController>();
            services.AddTransient<Controllers.Dashboard.InventoryDashboardController>();
            services.AddTransient<Controllers.Dashboard.InventoryRiskDashboardController>();
            services.AddTransient<Controllers.Dashboard.PurchasingDashboardController>();
            services.AddTransient<Controllers.Dashboard.CustomerDashboardController>();
            services.AddTransient<Controllers.Dashboard.SalesmanDashboardController>();
            services.AddTransient<Controllers.Dashboard.FieldActivityDashboardController>();
            services.AddTransient<Controllers.Dashboard.CollectionDashboardController>();
            services.AddTransient<Controllers.Dashboard.CashFlowForecastDashboardController>();
            services.AddTransient<Controllers.Dashboard.CustomerRiskForecastDashboardController>();
            services.AddTransient<Controllers.Dashboard.CollectionOptimizationDashboardController>();
            services.AddTransient<Controllers.Dashboard.InventoryForecastDashboardController>();
            services.AddTransient<Controllers.Dashboard.InventoryOptimizationDashboardController>();
            services.AddTransient<Controllers.Dashboard.LocationDashboardController>();
            services.AddTransient<Controllers.Reports.SalesReportController>();
            services.AddTransient<Controllers.Reports.InventoryReportController>();
            services.AddTransient<Controllers.Reports.PiutangReportController>();
            services.AddTransient<Controllers.Reports.PurchasingReportController>();

            return services;
        }
    }
}
