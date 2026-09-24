using btrade.application;
using btrade.application.UseCase;
using Nuna.Lib.AutoNumberHelper;
using Nuna.Lib.CleanArchHelper;
using Nuna.Lib.ValidationHelper;
using Scrutor;

namespace btrade.webapi.Configurations;

public static class ApplicationService
{
    private const string APPLICATION_ASSEMBLY = "Aptol.Application";

    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services
            .AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<ApplicationAssemblyAnchor>()) 
            .AddScoped<INunaCounterBL, NunaCounterBL>()
            .AddScoped<DateTimeProvider, DateTimeProvider>()
            //  TD-04 — the single owner of session-context resolution. It is not
            //  discoverable by the Scrutor scans below (it implements no scanned
            //  abstraction), so it is registered explicitly.
            .AddScoped<SessionContextResolver>();

        services
            .Scan(selector => selector
                .FromAssemblyOf<ApplicationAssemblyAnchor>()
                    .AddClasses(c => c.AssignableTo(typeof(INunaWriter<>)))
                    .UsingRegistrationStrategy(RegistrationStrategy.Skip)
                    .AsSelfWithInterfaces()
                    .WithScopedLifetime()
                .FromAssemblyOf<ApplicationAssemblyAnchor>()
                    .AddClasses(c => c.AssignableTo(typeof(INunaWriterWithReturn<>)))
                    .UsingRegistrationStrategy(RegistrationStrategy.Skip)
                    .AsSelfWithInterfaces()
                    .WithScopedLifetime()
                .FromAssemblyOf<ApplicationAssemblyAnchor>()
                    .AddClasses(c => c.AssignableTo(typeof(INunaBuilder<>)))
                    .UsingRegistrationStrategy(RegistrationStrategy.Skip)
                    .AsSelfWithInterfaces()
                    .WithScopedLifetime()
            );
        return services;
    }
}