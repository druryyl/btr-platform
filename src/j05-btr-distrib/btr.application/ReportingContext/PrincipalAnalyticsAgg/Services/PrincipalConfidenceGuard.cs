using System.Collections.Generic;
using btr.application.ReportingContext.EntityAnalyticsAgg.Contracts;
using btr.application.ReportingContext.EntityAnalyticsAgg.Models;
using btr.application.ReportingContext.PrincipalAnalyticsAgg.Models;

namespace btr.application.ReportingContext.PrincipalAnalyticsAgg.Services
{
    /// <summary>
    /// Applies the configurable confidence guards (PSOM-07 / GAP-007) to the time-aware
    /// ratio KPIs. Thresholds are read from the KPI Registry metadata registered by
    /// PSOM-03; no threshold literals live in calculation code.
    /// </summary>
    public class PrincipalConfidenceGuard
    {
        private readonly IKpiRegistry _kpiRegistry;

        public PrincipalConfidenceGuard(IKpiRegistry kpiRegistry)
        {
            _kpiRegistry = kpiRegistry;
        }

        public PrincipalPacingAchievementResult ApplyPacingElapsedDaysGuards(
            PrincipalPacingAchievementResult result,
            int elapsedDays)
        {
            if (result != null)
            {
                foreach (var row in result.Principals ?? new List<PrincipalPacingAchievementRow>())
                    ApplyPacingElapsedDaysGuard(row, elapsedDays);
            }

            return result;
        }

        public PrincipalYoyMtdGrowthResult ApplyYoyMtdBaseGuards(PrincipalYoyMtdGrowthResult result)
        {
            if (result != null)
            {
                foreach (var row in result.Principals ?? new List<PrincipalYoyMtdGrowthRow>())
                    ApplyYoyMtdBaseGuard(row);
            }

            return result;
        }

        public void ApplyPacingElapsedDaysGuard(PrincipalPacingAchievementRow row, int elapsedDays)
        {
            if (row == null)
                return;

            var minimum = ResolveMinimumElapsedDays(PrincipalKpiCatalog.PacingAchievementPercentageId);
            if (!IsElapsedDaysBelowMinimum(elapsedDays, minimum))
            {
                row.ConfidenceStatus = KpiConfidenceStatus.Normal;
                return;
            }

            row.ConfidenceStatus = KpiConfidenceStatus.LowConfidence;
            row.PacingAchievementPercentage = null;
        }

        public void ApplyYoyMtdBaseGuard(PrincipalYoyMtdGrowthRow row)
        {
            if (row == null)
                return;

            var minimum = ResolveMinimumBaseValue(PrincipalKpiCatalog.YoyMtdGrowthId);
            if (!IsBaseBelowMinimum(row.PriorYearMtdSalesOutAmount, minimum))
            {
                row.ConfidenceStatus = KpiConfidenceStatus.Normal;
                return;
            }

            row.ConfidenceStatus = KpiConfidenceStatus.LowConfidence;
            row.YoyMtdGrowthPercentage = null;
        }

        public static bool IsElapsedDaysBelowMinimum(int elapsedDays, int? minimumElapsedDays)
        {
            return minimumElapsedDays.HasValue && elapsedDays < minimumElapsedDays.Value;
        }

        public static bool IsBaseBelowMinimum(decimal? baseValue, decimal? minimumBaseValue)
        {
            return minimumBaseValue.HasValue && (!baseValue.HasValue || baseValue.Value < minimumBaseValue.Value);
        }

        private EntityKpiMetadata ResolveMetadata(string kpiId)
        {
            if (_kpiRegistry != null && _kpiRegistry.TryGetMetadata(kpiId, out var metadata))
                return metadata;

            return null;
        }

        private int? ResolveMinimumElapsedDays(string kpiId)
        {
            return ResolveMetadata(kpiId)?.MinimumElapsedDays;
        }

        private decimal? ResolveMinimumBaseValue(string kpiId)
        {
            return ResolveMetadata(kpiId)?.MinimumBaseValue;
        }
    }
}