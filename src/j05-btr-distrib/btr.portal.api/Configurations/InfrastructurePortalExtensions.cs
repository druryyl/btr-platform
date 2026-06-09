using btr.application.ReportingContext.DashboardCustomerAgg.Contracts;
using btr.application.ReportingContext.DashboardSalesmanAgg.Contracts;
using btr.application.ReportingContext.DashboardExecutiveAgg.Contracts;
using btr.application.ReportingContext.DashboardExecutiveAgg.Services;
using btr.application.ReportingContext.DashboardInventoryAgg.Contracts;
using btr.application.ReportingContext.DashboardInventoryRiskAgg.Contracts;
using btr.application.ReportingContext.DashboardOverviewAgg.Contracts;
using btr.application.ReportingContext.DashboardPiutangAgg.Contracts;
using btr.application.ReportingContext.DashboardPurchasingAgg.Contracts;
using btr.application.ReportingContext.DashboardSalesAgg.Contracts;
using btr.application.ReportingContext.DashboardSnapshotAgg.Contracts;
using btr.application.ReportingContext.InventoryReportAgg.Contracts;
using btr.application.ReportingContext.PiutangReportAgg.Contracts;
using btr.application.ReportingContext.PurchasingReportAgg.Contracts;
using btr.application.ReportingContext.SalesReportAgg.Contracts;
using btr.application.SalesContext.FakturInfo;
using btr.application.SalesContext.SalesOmzetAgg.Contracts;
using btr.application.SalesContext.SalesOmzetHealthWeeklyAgg.Contracts;
using btr.infrastructure;
using btr.infrastructure.Helpers;
using btr.infrastructure.ReportingContext.DashboardCustomerAgg;
using btr.infrastructure.ReportingContext.DashboardSalesmanAgg;
using btr.infrastructure.ReportingContext.DashboardExecutiveAgg;
using btr.infrastructure.ReportingContext.DashboardInventoryAgg;
using btr.infrastructure.ReportingContext.DashboardInventoryRiskAgg;
using btr.infrastructure.ReportingContext.DashboardPiutangAgg;
using btr.infrastructure.ReportingContext.DashboardPurchasingAgg;
using btr.infrastructure.ReportingContext.DashboardSalesAgg;
using btr.infrastructure.ReportingContext.DashboardSnapshotAgg;
using btr.infrastructure.ReportingContext.InventoryReportAgg;
using btr.infrastructure.ReportingContext.PiutangReportAgg;
using btr.infrastructure.ReportingContext.PurchasingReportAgg;
using btr.infrastructure.ReportingContext.SalesReportAgg;
using btr.infrastructure.SalesContext.FakturInfoAgg;
using btr.infrastructure.SalesContext.SalesOmzetAgg;
using btr.infrastructure.SalesContext.SalesOmzetHealthWeeklyAgg;
using btr.nuna.Application;
using btr.nuna.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Scrutor;

namespace btr.portal.api.Configurations
{
    public static class InfrastructurePortalExtensions
    {
        public static IServiceCollection AddInfrastructurePortal(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            services.Configure<DatabaseOptions>(configuration.GetSection(DatabaseOptions.SECTION_NAME));
            services.AddSingleton<IConnectionSettingProvider, JsonConnectionSettingProvider>();
            services.AddSingleton<ConnectionStringFactory>();
            services.AddScoped<INunaCounterDal, ParamNoDal>();

            services.AddScoped<ISalesOmzetSourceDal, SalesOmzetSourceDal>();
            services.AddScoped<ISalesOmzetTargetDal, SalesOmzetTargetDal>();
            services.AddScoped<ISalesOmzetHealthMetricsDal, SalesOmzetHealthMetricsDal>();
            services.AddScoped<IPiutangOpenBalanceDal, PiutangOpenBalanceDal>();
            services.AddScoped<IPiutangOpenBalanceWithSalesmanDal, PiutangOpenBalanceWithSalesmanDal>();
            services.AddScoped<IPiutangOpenBalanceWithWilayahDal, PiutangOpenBalanceWithWilayahDal>();
            services.AddScoped<IDashboardSnapshotRefreshLogDal, DashboardSnapshotRefreshLogDal>();
            services.AddScoped<IDashboardPiutangSnapshotDal, DashboardPiutangSnapshotDal>();
            services.AddScoped<IDashboardInventorySnapshotDal, DashboardInventorySnapshotDal>();
            services.AddScoped<IDashboardInventoryRiskSnapshotDal, DashboardInventoryRiskSnapshotDal>();
            services.AddScoped<IDashboardSalesSnapshotDal, DashboardSalesSnapshotDal>();
            services.AddScoped<IDashboardPurchasingSnapshotDal, DashboardPurchasingSnapshotDal>();
            services.AddScoped<IDashboardPurchasingManagementSnapshotDal, DashboardPurchasingManagementSnapshotDal>();
            services.AddScoped<IDashboardCustomerSnapshotDal, DashboardCustomerSnapshotDal>();
            services.AddScoped<IDashboardSalesmanSnapshotDal, DashboardSalesmanSnapshotDal>();
            services.AddScoped<IDashboardCollectionSnapshotDal, DashboardCollectionSnapshotDal>();
            services.AddScoped<IDashboardLocationSnapshotDal, DashboardLocationSnapshotDal>();
            services.AddScoped<ICustomerLastFakturDal, CustomerLastFakturDal>();
            services.AddScoped<IBrgLastFakturDal, BrgLastFakturDal>();
            services.AddScoped<IDashboardOverviewDal, DashboardOverviewDal>();
            services.AddScoped<IDashboardExecutiveDal, DashboardExecutiveDal>();
            services.AddScoped<DashboardExecutiveComposer>();
            services.AddScoped<IDashboardSalesDal, DashboardSalesDal>();
            services.AddScoped<IDashboardPurchasingDal, DashboardPurchasingDal>();
            services.AddScoped<IDashboardPiutangDal, DashboardPiutangDal>();
            services.AddScoped<IDashboardInventoryDal, DashboardInventoryDal>();
            services.AddScoped<IDashboardInventoryRiskDal, DashboardInventoryRiskDal>();
            services.AddScoped<IDashboardCustomerDal, DashboardCustomerDal>();
            services.AddScoped<IDashboardSalesmanDal, DashboardSalesmanDal>();
            services.AddScoped<IDashboardCollectionDal, DashboardCollectionDal>();
            services.AddScoped<IDashboardLocationDal, DashboardLocationDal>();
            services.AddScoped<ISalesReportDal, SalesReportDal>();
            services.AddScoped<IInventoryReportDal, InventoryReportDal>();
            services.AddScoped<IPiutangReportDal, PiutangReportDal>();
            services.AddScoped<IPurchasingReportDal, PurchasingReportDal>();

            services
                .Scan(selector => selector
                    .FromAssemblyOf<InfrastructureAssemblyAnchor>()
                        .AddClasses(c => c.AssignableTo(typeof(IInsert<>)))
                        .UsingRegistrationStrategy(RegistrationStrategy.Skip)
                        .AsSelfWithInterfaces()
                        .WithScopedLifetime()
                    .FromAssemblyOf<InfrastructureAssemblyAnchor>()
                        .AddClasses(c => c.AssignableTo(typeof(IUpdate<>)))
                        .UsingRegistrationStrategy(RegistrationStrategy.Skip)
                        .AsSelfWithInterfaces()
                        .WithScopedLifetime()
                    .FromAssemblyOf<InfrastructureAssemblyAnchor>()
                        .AddClasses(c => c.AssignableTo(typeof(IDelete<>)))
                        .UsingRegistrationStrategy(RegistrationStrategy.Skip)
                        .AsSelfWithInterfaces()
                        .WithScopedLifetime()
                    .FromAssemblyOf<InfrastructureAssemblyAnchor>()
                        .AddClasses(c => c.AssignableTo(typeof(IGetData<,>)))
                        .UsingRegistrationStrategy(RegistrationStrategy.Skip)
                        .AsSelfWithInterfaces()
                        .WithScopedLifetime()
                    .FromAssemblyOf<InfrastructureAssemblyAnchor>()
                        .AddClasses(c => c.AssignableTo(typeof(IListData<>)))
                        .UsingRegistrationStrategy(RegistrationStrategy.Skip)
                        .AsSelfWithInterfaces()
                        .WithScopedLifetime()
                    .FromAssemblyOf<InfrastructureAssemblyAnchor>()
                        .AddClasses(c => c.AssignableTo(typeof(IListData<,>)))
                        .UsingRegistrationStrategy(RegistrationStrategy.Skip)
                        .AsSelfWithInterfaces()
                        .WithScopedLifetime()
                    .FromAssemblyOf<InfrastructureAssemblyAnchor>()
                        .AddClasses(c => c.AssignableTo(typeof(IListData<,,>)))
                        .UsingRegistrationStrategy(RegistrationStrategy.Skip)
                        .AsSelfWithInterfaces()
                        .WithScopedLifetime()
                    .FromAssemblyOf<InfrastructureAssemblyAnchor>()
                        .AddClasses(c => c.AssignableTo(typeof(IListData<,,,>)))
                        .UsingRegistrationStrategy(RegistrationStrategy.Skip)
                        .AsSelfWithInterfaces()
                        .WithScopedLifetime()
                    .FromAssemblyOf<InfrastructureAssemblyAnchor>()
                        .AddClasses(c => c.AssignableTo(typeof(INunaService<,>)))
                        .UsingRegistrationStrategy(RegistrationStrategy.Skip)
                        .AsSelfWithInterfaces()
                        .WithScopedLifetime()
                    .FromAssemblyOf<InfrastructureAssemblyAnchor>()
                        .AddClasses(c => c.AssignableTo(typeof(INunaService<>)))
                        .UsingRegistrationStrategy(RegistrationStrategy.Skip)
                        .AsSelfWithInterfaces()
                        .WithScopedLifetime()
                    .FromAssemblyOf<InfrastructureAssemblyAnchor>()
                        .AddClasses(c => c.AssignableTo(typeof(ISaveChange<>)))
                        .UsingRegistrationStrategy(RegistrationStrategy.Skip)
                        .AsSelfWithInterfaces()
                        .WithScopedLifetime()
                    .FromAssemblyOf<InfrastructureAssemblyAnchor>()
                        .AddClasses(c => c.AssignableTo(typeof(IDeleteEntity<>)))
                        .UsingRegistrationStrategy(RegistrationStrategy.Skip)
                        .AsSelfWithInterfaces()
                        .WithScopedLifetime()
                    .FromAssemblyOf<InfrastructureAssemblyAnchor>()
                        .AddClasses(c => c.AssignableTo(typeof(ILoadEntity<,>)))
                        .UsingRegistrationStrategy(RegistrationStrategy.Skip)
                        .AsSelfWithInterfaces()
                        .WithScopedLifetime());

            return services;
        }
    }
}
