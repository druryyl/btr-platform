using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web.Http;
using btr.application.ReportingContext.DashboardSnapshotAgg;
using btr.application.ReportingContext.DashboardSnapshotAgg.Contracts;
using btr.application.ReportingContext.DashboardSnapshotAgg.Models;
using btr.portal.api.Models;
using Microsoft.Extensions.Options;
using NLog;

namespace btr.portal.api.Controllers
{
    [RoutePrefix("api/health")]
    public class HealthController : ApiController
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private readonly IDashboardSnapshotRefreshLogDal _refreshLogDal;
        private readonly DashboardSnapshotOptions _snapshotOptions;

        public HealthController(
            IDashboardSnapshotRefreshLogDal refreshLogDal,
            IOptions<DashboardSnapshotOptions> snapshotOptions)
        {
            _refreshLogDal = refreshLogDal;
            _snapshotOptions = snapshotOptions?.Value ?? new DashboardSnapshotOptions();
        }

        [HttpGet, Route("")]
        public IHttpActionResult Get()
        {
            Logger.Info("Health check requested");
            var version = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "unknown";

            var data = new HealthData
            {
                Status = "ok",
                Service = "btr.portal.api",
                Version = version,
                TimestampUtc = DateTime.UtcNow
            };

            return Ok(ApiResponse<HealthData>.Success(data));
        }

        [HttpGet, Route("dashboard-snapshots")]
        public IHttpActionResult GetDashboardSnapshots()
        {
            Logger.Info("Dashboard snapshot health check requested");

            var latest = _refreshLogDal.GetLatestPerDomain();
            var domains = BuildDomainStatuses(latest);

            var data = new DashboardSnapshotHealthData
            {
                Status = DashboardSnapshotHealthStatusResolver.ResolveOverallStatus(
                    domains.Select(d => d.LastRefresh?.Status).ToList()),
                CheckedAtUtc = DateTime.UtcNow,
                Domains = domains
            };

            return Ok(ApiResponse<DashboardSnapshotHealthData>.Success(data));
        }

        private IList<DashboardSnapshotDomainHealth> BuildDomainStatuses(
            IReadOnlyList<DashboardSnapshotRefreshStatusModel> latest)
        {
            var byDomain = latest.ToDictionary(x => x.Domain, StringComparer.OrdinalIgnoreCase);
            var domains = new[] { "Piutang", "Inventory", "Sales", "Purchasing" };
            var result = new List<DashboardSnapshotDomainHealth>();

            foreach (var domain in domains)
            {
                byDomain.TryGetValue(domain, out var status);

                result.Add(new DashboardSnapshotDomainHealth
                {
                    Domain = domain,
                    IntervalMinutes = GetIntervalMinutes(domain),
                    LastRefresh = status == null
                        ? null
                        : new DashboardSnapshotLastRefresh
                        {
                            RefreshLogId = status.RefreshLogId,
                            StartedAt = status.StartedAt,
                            CompletedAt = status.CompletedAt,
                            Status = status.Status,
                            DurationMs = status.DurationMs,
                            ErrorMessage = status.ErrorMessage,
                            TriggeredBy = status.TriggeredBy
                        }
                });
            }

            return result;
        }

        private int GetIntervalMinutes(string domain)
        {
            switch (domain)
            {
                case "Piutang":
                    return _snapshotOptions.PiutangIntervalMinutes;
                case "Inventory":
                    return _snapshotOptions.InventoryIntervalMinutes;
                case "Sales":
                    return _snapshotOptions.SalesIntervalMinutes;
                case "Purchasing":
                    return _snapshotOptions.PurchasingIntervalMinutes;
                default:
                    return 0;
            }
        }
    }

    public class HealthData
    {
        public string Status { get; set; }
        public string Service { get; set; }
        public string Version { get; set; }
        public DateTime TimestampUtc { get; set; }
    }

    public class DashboardSnapshotHealthData
    {
        public string Status { get; set; }

        public DateTime CheckedAtUtc { get; set; }

        public IList<DashboardSnapshotDomainHealth> Domains { get; set; }
    }

    public class DashboardSnapshotDomainHealth
    {
        public string Domain { get; set; }

        public int IntervalMinutes { get; set; }

        public DashboardSnapshotLastRefresh LastRefresh { get; set; }
    }

    public class DashboardSnapshotLastRefresh
    {
        public string RefreshLogId { get; set; }

        public DateTime StartedAt { get; set; }

        public DateTime? CompletedAt { get; set; }

        public string Status { get; set; }

        public int DurationMs { get; set; }

        public string ErrorMessage { get; set; }

        public string TriggeredBy { get; set; }
    }
}
