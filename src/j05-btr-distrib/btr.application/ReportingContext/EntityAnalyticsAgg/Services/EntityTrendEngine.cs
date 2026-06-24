using System;

using System.Collections.Generic;

using System.Globalization;

using System.Linq;

using btr.application.ReportingContext.EntityAnalyticsAgg.Contracts;

using btr.application.ReportingContext.EntityAnalyticsAgg.Models;

using btr.application.ReportingContext.EntityAnalyticsAgg.Models.Snapshot;

using btr.application.ReportingContext.EntityAnalyticsAgg.Options;

using btr.application.ReportingContext.EntityAnalyticsAgg.Queries;

using Microsoft.Extensions.Options;



namespace btr.application.ReportingContext.EntityAnalyticsAgg.Services

{

    public class EntityTrendEngine : IEntityTrendEngine

    {

        private readonly IEntityAnalyticsRepository _repository;

        private readonly IKpiRegistry _kpiRegistry;

        private readonly IEntityTypeRegistry _entityTypes;

        private readonly EntityAnalyticsOptions _options;



        public EntityTrendEngine(

            IEntityAnalyticsRepository repository,

            IKpiRegistry kpiRegistry,

            IEntityTypeRegistry entityTypes,

            IOptions<EntityAnalyticsOptions> options)

        {

            _repository = repository;

            _kpiRegistry = kpiRegistry;

            _entityTypes = entityTypes;

            _options = options?.Value ?? new EntityAnalyticsOptions();

        }



        public ProfileTrendSectionDto BuildTrendSection(string entityType, string entityId)

        {

            var trendKpis = ResolveTrendEligibleKpis(entityType);

            if (trendKpis.Count == 0)

            {

                return new ProfileTrendSectionDto

                {

                    IsAvailable = false,

                    UnavailableReason = EntityAnalyticsUnavailableReasons.NoRegisteredKpis,

                    Series = new List<ProfileTrendSeriesDto>()

                };

            }



            var windowMonths = Math.Min(

                EntityAnalyticsConstants.DefaultTrendWindowMonths,

                _options.HistoryRetentionMonths > 0 ? _options.HistoryRetentionMonths : 36);



            var periods = _repository.GetLatestPeriods(entityType, entityId, windowMonths);

            if (periods.Count == 0)

            {

                return new ProfileTrendSectionDto

                {

                    IsAvailable = false,

                    UnavailableReason = EntityAnalyticsUnavailableReasons.NoSnapshotData,

                    Series = new List<ProfileTrendSeriesDto>()

                };

            }



            var from = periods[0];

            var to = periods[periods.Count - 1];

            var history = _repository.GetHistory(

                entityType,

                entityId,

                from.Year,

                from.Month,

                to.Year,

                to.Month);



            var series = new List<ProfileTrendSeriesDto>();

            foreach (var kpi in trendKpis)

            {

                var kpiRows = history

                    .Where(r => string.Equals(r.KpiId, kpi.KpiId, StringComparison.OrdinalIgnoreCase))

                    .OrderBy(r => r.PeriodYear)

                    .ThenBy(r => r.PeriodMonth)

                    .ToList();



                if (kpiRows.Count == 0)

                    continue;



                var periodSemantics = kpiRows

                    .Select(r => r.PeriodSemantics)

                    .FirstOrDefault(s => !string.IsNullOrWhiteSpace(s))

                    ?? kpi.PeriodSemantics;



                series.Add(new ProfileTrendSeriesDto

                {

                    KpiId = kpi.KpiId,

                    DisplayName = kpi.DisplayName,

                    PeriodSemantics = periodSemantics,

                    Unit = kpi.Unit,

                    Points = kpiRows.Select(r => new ProfileTrendPointDto

                    {

                        PeriodYear = r.PeriodYear,

                        PeriodMonth = r.PeriodMonth,

                        Value = r.NumericValue,

                        IsClosed = r.IsClosed,

                        PeriodLabel = BuildPeriodLabel(r.PeriodYear, r.PeriodMonth, r.IsClosed, periodSemantics)

                    }).ToList()

                });

            }



            return new ProfileTrendSectionDto

            {

                IsAvailable = series.Count > 0,

                UnavailableReason = series.Count > 0

                    ? null

                    : EntityAnalyticsUnavailableReasons.NoSnapshotData,

                Series = series

            };

        }



        private IReadOnlyList<EntityKpiMetadata> ResolveTrendEligibleKpis(string entityType)

        {

            var packId = _entityTypes.TryGet(entityType, out var registration)

                ? registration.KpiPackId

                : null;



            if (string.IsNullOrWhiteSpace(packId))

                return Array.Empty<EntityKpiMetadata>();



            return _kpiRegistry.ResolvePackMetadata(packId)

                .Where(m => m.TrendEligible)

                .ToList();

        }



        private static string BuildPeriodLabel(int year, int month, bool isClosed, string periodSemantics)

        {

            var monthName = CultureInfo.InvariantCulture.DateTimeFormat.GetAbbreviatedMonthName(month);

            var label = $"{monthName} {year}";



            if (!isClosed && string.Equals(periodSemantics, "MTD", StringComparison.OrdinalIgnoreCase))

                return $"{label} (MTD)";



            return label;

        }

    }

}


