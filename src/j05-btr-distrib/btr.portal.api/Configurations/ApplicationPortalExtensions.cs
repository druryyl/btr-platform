using System.Reflection;
using btr.application;
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

            return services;
        }
    }
}
