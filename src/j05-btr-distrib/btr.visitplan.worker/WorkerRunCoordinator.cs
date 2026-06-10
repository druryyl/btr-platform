using System;
using btr.application.SalesContext.VisitPlanAgg.UseCases;
using btr.infrastructure.Helpers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NLog;

namespace btr.visitplan.worker
{
    internal sealed class WorkerRunCoordinator
    {
        private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();

        public int Run(string[] args)
        {
            LogManager.Setup().LoadConfigurationFromFile("NLog.config");
            var triggeredBy = ParseRequiredOption(args, "--triggered-by", "Scheduler");

            try
            {
                ValidateOptions(triggeredBy);
                Logger.Info("Starting visit plan horizon maintenance. TriggeredBy={TriggeredBy}", triggeredBy);

                var configuration = new ConfigurationBuilder()
                    .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
                    .AddJsonFile($"appsettings.{Environment.MachineName}.json", optional: true)
                    .Build();

                var serviceProvider = WorkerDependencyConfig.Configure(configuration);
                ConnStringHelper.Initialize(serviceProvider.GetRequiredService<ConnectionStringFactory>());
                DatabaseConnectionValidator.Validate(serviceProvider.GetRequiredService<ConnectionStringFactory>());

                using (var scope = serviceProvider.CreateScope())
                {
                    var worker = scope.ServiceProvider.GetRequiredService<IMaintainVisitPlanHorizonWorker>();
                    worker.Execute(new MaintainVisitPlanHorizonRequest { TriggeredBy = triggeredBy });
                }

                Logger.Info("Visit plan horizon maintenance completed successfully.");
                if (string.Equals(triggeredBy, "Manual", StringComparison.OrdinalIgnoreCase))
                {
                    Console.Out.WriteLine("Press any key to exit...");
                    Console.ReadKey(intercept: true);
                }

                return 0;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Visit plan horizon maintenance failed.");
                if (string.Equals(triggeredBy, "Manual", StringComparison.OrdinalIgnoreCase))
                {
                    Console.Out.WriteLine("Press any key to exit...");
                    Console.ReadKey(intercept: true);
                }

                return 1;
            }
            finally
            {
                LogManager.Shutdown();
            }
        }

        private static void ValidateOptions(string triggeredBy)
        {
            if (!string.Equals(triggeredBy, "Scheduler", StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(triggeredBy, "Manual", StringComparison.OrdinalIgnoreCase))
            {
                throw new ArgumentException(
                    $"Invalid --triggered-by '{triggeredBy}'. Expected Scheduler or Manual.");
            }
        }

        private static string ParseRequiredOption(string[] args, string name, string defaultValue)
        {
            for (var i = 0; i < args.Length; i++)
            {
                if (!string.Equals(args[i], name, StringComparison.OrdinalIgnoreCase))
                    continue;

                if (i + 1 >= args.Length || args[i + 1].StartsWith("--", StringComparison.Ordinal))
                    throw new ArgumentException($"Missing value for {name}.");

                return args[i + 1];
            }

            return defaultValue;
        }
    }
}
