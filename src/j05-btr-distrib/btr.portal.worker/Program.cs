using System;
using System.Collections.Generic;
using btr.application.ReportingContext.DashboardSnapshotAgg.UseCases;
using btr.infrastructure.Helpers;
using btr.portal.api.Configurations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NLog;

namespace btr.portal.worker
{
    internal static class Program
    {
        private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();

        private static readonly HashSet<string> ValidDomains =
            new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "All", "Sales", "Piutang", "Inventory"
            };

        private static readonly HashSet<string> ValidTriggers =
            new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "Scheduler", "Manual"
            };

        private static int Main(string[] args)
        {
            LogManager.Setup().LoadConfigurationFromFile("NLog.config");

            try
            {
                var domain = ParseRequiredOption(args, "--domain", "All");
                var triggeredBy = ParseRequiredOption(args, "--triggered-by", "Scheduler");

                if (!ValidDomains.Contains(domain))
                    throw new ArgumentException(
                        $"Invalid --domain '{domain}'. Expected All, Sales, Piutang, or Inventory.");

                if (!ValidTriggers.Contains(triggeredBy))
                    throw new ArgumentException(
                        $"Invalid --triggered-by '{triggeredBy}'. Expected Scheduler or Manual.");

                Logger.Info(
                    "Starting dashboard snapshot refresh. Domain={Domain}, TriggeredBy={TriggeredBy}",
                    domain,
                    triggeredBy);

                var configuration = BuildConfiguration();
                var serviceProvider = WorkerDependencyConfig.Configure(configuration);

                ConnStringHelper.Initialize(
                    serviceProvider.GetRequiredService<ConnectionStringFactory>());

                using (var scope = serviceProvider.CreateScope())
                {
                    ExecuteRefresh(scope.ServiceProvider, domain, triggeredBy);
                }

                Logger.Info("Dashboard snapshot refresh completed successfully. Domain={Domain}", domain);
                return 0;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Dashboard snapshot refresh failed.");
                return 1;
            }
            finally
            {
                LogManager.Shutdown();
            }
        }

        private static IConfiguration BuildConfiguration()
        {
            return new ConfigurationBuilder()
                .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
                .AddJsonFile($"appsettings.{Environment.MachineName}.json", optional: true)
                .Build();
        }

        private static void ExecuteRefresh(
            IServiceProvider serviceProvider,
            string domain,
            string triggeredBy)
        {
            switch (domain.ToUpperInvariant())
            {
                case "ALL":
                    var allWorker = serviceProvider.GetRequiredService<IRefreshAllDashboardSnapshotsWorker>();
                    allWorker.Execute(new RefreshAllDashboardSnapshotsRequest
                    {
                        TriggeredBy = triggeredBy
                    });
                    break;

                case "PIUTANG":
                    var piutangWorker =
                        serviceProvider.GetRequiredService<IRefreshDashboardPiutangSnapshotWorker>();
                    piutangWorker.Execute(new RefreshDashboardPiutangSnapshotRequest
                    {
                        TriggeredBy = triggeredBy
                    });
                    break;

                case "INVENTORY":
                    var inventoryWorker =
                        serviceProvider.GetRequiredService<IRefreshDashboardInventorySnapshotWorker>();
                    inventoryWorker.Execute(new RefreshDashboardInventorySnapshotRequest
                    {
                        TriggeredBy = triggeredBy
                    });
                    break;

                case "SALES":
                    var salesWorker =
                        serviceProvider.GetRequiredService<IRefreshDashboardSalesSnapshotWorker>();
                    salesWorker.Execute(new RefreshDashboardSalesSnapshotRequest
                    {
                        TriggeredBy = triggeredBy
                    });
                    break;

                default:
                    throw new ArgumentException($"Unsupported domain '{domain}'.");
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
