using System.Reflection;
using btr.application;
using btr.application.Portal;
using btr.application.ReportingContext.DashboardFieldActivityAgg.Services;
using btr.application.ReportingContext.DashboardSnapshotAgg;
using btr.application.ReportingContext.DashboardSnapshotAgg.Services;
using btr.application.ReportingContext.EntityAnalyticsAgg.Contracts;
using btr.application.ReportingContext.EntityAnalyticsAgg.Backfill.Contracts;
using btr.application.ReportingContext.EntityAnalyticsAgg.Backfill.Services;
using btr.application.ReportingContext.EntityAnalyticsAgg.Options;
using btr.application.ReportingContext.EntityAnalyticsAgg.Producers;
using btr.application.ReportingContext.EntityAnalyticsAgg.Registrars;
using btr.application.ReportingContext.EntityAnalyticsAgg.Services;
using btr.application.SalesContext.SalesOmzetAgg.Contracts;
using btr.application.SalesContext.SalesOmzetAgg.Policies;
using btr.application.SalesContext.SalesOmzetAgg.Services;
using btr.application.SalesContext.SalesOmzetHealthWeeklyAgg.Policies;
using btr.application.SalesContext.SalesOmzetHealthWeeklyAgg.Services;
using btr.application.SupportContext.TglJamAgg;
using btr.infrastructure.SupportContext.TglJamAgg;
using btr.nuna.Application;
using btr.nuna.Domain;
using FluentValidation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Scrutor;

namespace btr.portal.api.Configurations
{
    public static class ApplicationPortalExtensions
    {
        public static IServiceCollection AddApplicationPortal(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            services.AddMediatR(cfg => cfg
                .RegisterServicesFromAssembly(typeof(ApplicationAssemblyAnchor).Assembly));
            services.AddValidatorsFromAssembly(Assembly.Load("btr.application"));
            services.AddScoped<INunaCounterBL, NunaCounterBL>();
            services.AddScoped<DateTimeProvider, DateTimeProvider>();
            services.AddScoped<ITglJamDal, TglJamDal>();
            services.Configure<DashboardSnapshotOptions>(
                configuration.GetSection(DashboardSnapshotOptions.SECTION_NAME));
            services.Configure<FieldActivityOptions>(
                configuration.GetSection(FieldActivityOptions.SECTION_NAME));
            services.Configure<PresentationOptions>(
                configuration.GetSection(PresentationOptions.SECTION_NAME));
            services.Configure<EntityAnalyticsOptions>(
                configuration.GetSection(EntityAnalyticsOptions.SECTION_NAME));
            services.AddSingleton<IPresentationModeService, PresentationModeService>();
            services.AddScoped<IBusinessDateProvider, PresentationBusinessDateProvider>();
            services.AddScoped(sp =>
                sp.GetRequiredService<IOptions<DashboardSnapshotOptions>>().Value);

            services
                .Scan(selector => selector
                    .FromAssemblyOf<ApplicationAssemblyAnchor>()
                        .AddClasses(c => c.AssignableTo(typeof(INunaWriter<>)))
                        .UsingRegistrationStrategy(RegistrationStrategy.Skip)
                        .AsSelfWithInterfaces()
                        .WithScopedLifetime()
                    .FromAssemblyOf<ApplicationAssemblyAnchor>()
                        .AddClasses(c => c.AssignableTo(typeof(INunaWriter2<>)))
                        .UsingRegistrationStrategy(RegistrationStrategy.Skip)
                        .AsSelfWithInterfaces()
                        .WithScopedLifetime()
                    .FromAssemblyOf<ApplicationAssemblyAnchor>()
                        .AddClasses(c => c.AssignableTo(typeof(INunaService<,>)))
                        .UsingRegistrationStrategy(RegistrationStrategy.Skip)
                        .AsSelfWithInterfaces()
                        .WithScopedLifetime()
                    .FromAssemblyOf<ApplicationAssemblyAnchor>()
                        .AddClasses(c => c.AssignableTo(typeof(INunaService<>)))
                        .UsingRegistrationStrategy(RegistrationStrategy.Skip)
                        .AsSelfWithInterfaces()
                        .WithScopedLifetime()
                    .FromAssemblyOf<ApplicationAssemblyAnchor>()
                        .AddClasses(c => c.AssignableTo(typeof(INunaServiceVoid<>)))
                        .UsingRegistrationStrategy(RegistrationStrategy.Skip)
                        .AsSelfWithInterfaces()
                        .WithScopedLifetime()
                    .FromAssemblyOf<ApplicationAssemblyAnchor>()
                        .AddClasses(c => c.AssignableTo(typeof(INunaBuilder<>)))
                        .UsingRegistrationStrategy(RegistrationStrategy.Skip)
                        .AsSelfWithInterfaces()
                        .WithScopedLifetime());

            services.AddScoped<ISalesOmzetEligibilityPolicy, SalesOmzetEligibilityPolicy>();
            services.AddScoped<ISalesOmzetSaleKindPolicy, SalesOmzetSaleKindPolicy>();
            services.AddScoped<ISalesOmzetStatusPolicy, SalesOmzetStatusPolicy>();
            services.AddScoped<ISalesOmzetPeriodPolicy, SalesOmzetPeriodPolicy>();
            services.AddScoped<ISalesOmzetChartAmountPolicy, SalesOmzetChartAmountPolicy>();
            services.AddScoped<ISalesOmzetChartSummaryBuilder, SalesOmzetChartSummaryBuilder>();
            services.AddScoped<ISalesOmzetTargetResolver, SalesOmzetTargetResolver>();
            services.AddScoped<ISalesOmzetSnapshotBuilder, SalesOmzetSnapshotBuilder>();
            services.AddScoped<ISalesOmzetLinker, SalesOmzetLinker>();
            services.AddScoped<IIsoWeekCalendar, IsoWeekCalendar>();
            services.AddScoped<ISalesOmzetHealthPolicy, SalesOmzetHealthPolicy>();
            services.AddScoped<ISalesOmzetReportHealthResolver, SalesOmzetReportHealthResolver>();
            services.AddScoped<DashboardPiutangAggregator>();
            services.AddScoped<DashboardInventoryAggregator>();
            services.AddScoped<DashboardInventoryRiskAggregator>();
            services.AddScoped<DashboardSalesFakturAggregator>();
            services.AddScoped<DashboardSalesForecastAggregator>();
            services.AddScoped<DashboardPurchasingInvoiceAggregator>();
            services.AddScoped<DashboardPurchasingManagementAggregator>();
            services.AddScoped<DashboardCustomerAggregator>();
            services.AddScoped<DashboardSalesmanAggregator>();
            services.AddScoped<DashboardCollectionAggregator>();
            services.AddScoped<DashboardCashFlowForecastAggregator>();
            services.AddScoped<DashboardCustomerRiskForecastAggregator>();
            services.AddScoped<DashboardCollectionOptimizationAggregator>();
            services.AddScoped<DashboardCustomerPortfolioAggregator>();
            services.AddScoped<DashboardCustomerRelationshipAggregator>();
            services.AddScoped<DashboardSalesmanRelationshipAggregator>();
            services.AddScoped<DashboardSupplierRelationshipAggregator>();
            services.AddScoped<DashboardItemRelationshipAggregator>();
            services.AddScoped<DashboardItemPortfolioBuilder>();
            services.AddScoped<DashboardInventoryForecastAggregator>();
            services.AddScoped<DashboardInventoryOptimizationAggregator>();
            services.AddScoped<DashboardLocationAggregator>();

            services.AddSingleton<IEntityTypeRegistry, EntityTypeRegistry>();
            services.AddSingleton<IDimensionLabelRegistry, EntityAnalyticsDimensionLabelRegistry>();
            services.AddSingleton<IAttentionSignalRegistry, EntityAttentionSignalRegistry>();
            services.AddSingleton<IRelationshipDefinitionRegistry, EntityRelationshipDefinitionRegistry>();
            services.AddSingleton<IEntityAnalyticsRegistrar, EntityAnalyticsPlatformRegistrar>();
            services.AddSingleton<IEntityAnalyticsRegistrar, CustomerEntityAnalyticsRegistrar>();
            services.AddSingleton<IEntityAnalyticsRegistrar, SalesmanEntityAnalyticsRegistrar>();
            services.AddSingleton<IEntityAnalyticsRegistrar, SupplierEntityAnalyticsRegistrar>();
            services.AddSingleton<IEntityAnalyticsRegistrar, ItemEntityAnalyticsRegistrar>();
            services.AddSingleton<IKpiRegistry, EntityAnalyticsKpiRegistry>();
            services.AddSingleton<EntityKpiEnvelopeFormatter>();
            services.AddScoped<IEntityTrendEngine, EntityTrendEngine>();
            services.AddScoped<IEntityRankingEngine, EntityRankingEngine>();
            services.AddScoped<IEntityRadarEngine, EntityRadarEngine>();
            services.AddScoped<IEntityAttentionEngine, EntityAttentionEngine>();
            services.AddScoped<IEntityRelationshipEngine, EntityRelationshipEngine>();
            services.AddScoped<IEntityComparisonEngine, EntityComparisonEngine>();
            services.AddScoped<IEntityAnalyticsMonthCloseService, EntityAnalyticsMonthCloseService>();
            services.AddSingleton(sp => new EntityAnalyticsRegistryBootstrap(
                sp.GetRequiredService<IEntityTypeRegistry>(),
                sp.GetRequiredService<IKpiRegistry>(),
                sp.GetRequiredService<IDimensionLabelRegistry>(),
                sp.GetRequiredService<IAttentionSignalRegistry>(),
                sp.GetRequiredService<IRelationshipDefinitionRegistry>(),
                sp.GetServices<IEntityAnalyticsRegistrar>()));
            services.AddScoped<IEntityProfileBuilder, EntityPerformanceProfileComposer>();
            services.AddScoped<IEntityAnalyticsService, EntityAnalyticsService>();
            services.AddScoped<EntityAnalyticsProducerOrchestrator>();
            services.AddScoped<IEntityAnalyticsBackfillOrchestrator, EntityAnalyticsBackfillOrchestrator>();
            services.AddScoped<IEntityAnalyticsProducer, CustomerEntityAnalyticsProducer>();
            services.AddScoped<IEntityAnalyticsProducer, SalesmanEntityAnalyticsProducer>();
            services.AddScoped<IEntityAnalyticsProducer, SupplierEntityAnalyticsProducer>();
            services.AddScoped<IEntityAnalyticsProducer, ItemEntityAnalyticsProducer>();
            services
                .Scan(selector => selector
                    .FromAssemblyOf<ApplicationAssemblyAnchor>()
                    .AddClasses(c => c.AssignableTo<IEntityAnalyticsProducer>())
                    .UsingRegistrationStrategy(RegistrationStrategy.Skip)
                    .AsImplementedInterfaces()
                    .WithScopedLifetime()
                    .AddClasses(c => c.AssignableTo<IEntityProfileEvidenceResolver>())
                    .UsingRegistrationStrategy(RegistrationStrategy.Skip)
                    .AsImplementedInterfaces()
                    .WithScopedLifetime());

            return services;
        }
    }
}
