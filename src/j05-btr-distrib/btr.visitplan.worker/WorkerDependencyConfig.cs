using System;
using System.Reflection;
using btr.application;
using btr.application.SalesContext.VisitPlanAgg;
using btr.application.SalesContext.VisitPlanAgg.Services;
using btr.application.SupportContext.TglJamAgg;
using btr.infrastructure;
using btr.infrastructure.Helpers;
using btr.infrastructure.SalesContext.VisitPlanAgg;
using btr.infrastructure.SupportContext.TglJamAgg;
using btr.nuna.Application;
using btr.nuna.Domain;
using btr.nuna.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Scrutor;

namespace btr.visitplan.worker
{
    public static class WorkerDependencyConfig
    {
        public static IServiceProvider Configure(IConfiguration configuration)
        {
            var services = new ServiceCollection();
            services.AddSingleton(configuration);

            services.AddScoped<INunaCounterBL, NunaCounterBL>();
            services.AddScoped<DateTimeProvider, DateTimeProvider>();
            services.AddScoped<ITglJamDal, TglJamDal>();

            services
                .Scan(selector => selector
                    .FromAssemblyOf<ApplicationAssemblyAnchor>()
                        .AddClasses(c => c.AssignableTo(typeof(INunaServiceVoid<>)))
                        .UsingRegistrationStrategy(RegistrationStrategy.Skip)
                        .AsSelfWithInterfaces()
                        .WithScopedLifetime());

            services.AddScoped<IRuteCycleCalendar, RuteCycleCalendar>();
            services.AddScoped<IEffectiveVisitPlanResolver, EffectiveVisitPlanResolver>();
            services.AddScoped<IVisitPlanExceptionWriter, VisitPlanExceptionWriter>();
            services.AddScoped<IVisitPlanDal, VisitPlanDal>();
            services.AddScoped<IEffectiveVisitPlanDal, EffectiveVisitPlanDal>();

            services.Configure<DatabaseOptions>(configuration.GetSection(DatabaseOptions.SECTION_NAME));
            services.AddSingleton<IConnectionSettingProvider, JsonConnectionSettingProvider>();
            services.AddSingleton<ConnectionStringFactory>();
            services.AddScoped<INunaCounterDal, ParamNoDal>();

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
                        .WithScopedLifetime());

            return services.BuildServiceProvider();
        }
    }
}
