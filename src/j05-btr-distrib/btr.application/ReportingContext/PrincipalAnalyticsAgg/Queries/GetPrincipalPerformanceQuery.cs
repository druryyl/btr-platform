using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using btr.application.ReportingContext.PrincipalAnalyticsAgg.Contracts;
using btr.application.ReportingContext.PrincipalAnalyticsAgg.Models;
using MediatR;

namespace btr.application.ReportingContext.PrincipalAnalyticsAgg.Queries
{
    public class GetPrincipalPerformanceQuery : IRequest<PrincipalPerformanceResponse>
    {
    }

    public class PrincipalPerformanceResponse
    {
        public bool IsAvailable { get; set; }

        public string KpiId { get; set; }

        public string KpiName { get; set; }

        public int PeriodYear { get; set; }

        public int PeriodMonth { get; set; }

        public DateTime? GeneratedAt { get; set; }

        public decimal PrincipalSalesOutAmount { get; set; }

        public int UnknownPrincipalExceptionCount { get; set; }

        public string TargetKpiId { get; set; }

        public decimal? PrincipalTargetAmount { get; set; }

        public string AchievementAmountKpiId { get; set; }

        public string AchievementPercentageKpiId { get; set; }

        public decimal? AchievementAmount { get; set; }

        public decimal? AchievementPercentage { get; set; }

        public bool TargetAchievementIsAvailable { get; set; }

        public int MissingTargetExceptionCount { get; set; }

        public IList<string> Disclosures { get; set; }
            = new List<string>();

        public IList<PrincipalPerformanceRankingItem> Ranking { get; set; }
            = new List<PrincipalPerformanceRankingItem>();
    }

    public class PrincipalPerformanceRankingItem
    {
        public int Rank { get; set; }

        public string PrincipalName { get; set; }

        public string SupplierId { get; set; }

        public string KpiId { get; set; }

        public decimal PrincipalSalesOutAmount { get; set; }

        public string TargetKpiId { get; set; }

        public decimal? PrincipalTargetAmount { get; set; }

        public string AchievementAmountKpiId { get; set; }

        public string AchievementPercentageKpiId { get; set; }

        public decimal? AchievementAmount { get; set; }

        public decimal? AchievementPercentage { get; set; }
    }

    public class GetPrincipalPerformanceHandler
        : IRequestHandler<GetPrincipalPerformanceQuery, PrincipalPerformanceResponse>
    {
        private readonly IPrincipalSalesOutSnapshotDal _snapshotDal;
        private readonly IPrincipalTargetSnapshotDal _targetSnapshotDal;
        private readonly IPrincipalAchievementSnapshotDal _achievementSnapshotDal;

        public GetPrincipalPerformanceHandler(
            IPrincipalSalesOutSnapshotDal snapshotDal,
            IPrincipalTargetSnapshotDal targetSnapshotDal,
            IPrincipalAchievementSnapshotDal achievementSnapshotDal)
        {
            _snapshotDal = snapshotDal;
            _targetSnapshotDal = targetSnapshotDal;
            _achievementSnapshotDal = achievementSnapshotDal;
        }

        public Task<PrincipalPerformanceResponse> Handle(
            GetPrincipalPerformanceQuery request,
            CancellationToken cancellationToken)
        {
            return Task.FromResult(PrincipalPerformanceComposer.Compose(
                _snapshotDal.GetCurrent(),
                _targetSnapshotDal.GetCurrent(),
                _achievementSnapshotDal.GetCurrent()));
        }
    }

    public static class PrincipalPerformanceComposer
    {
        public static PrincipalPerformanceResponse Compose(PrincipalSalesOutAggregateResult snapshot)
        {
            return Compose(snapshot, null, null);
        }

        public static PrincipalPerformanceResponse Compose(
            PrincipalSalesOutAggregateResult snapshot,
            PrincipalTargetAggregateResult target,
            PrincipalAchievementResult achievement)
        {
            var response = new PrincipalPerformanceResponse
            {
                KpiId = PrincipalKpiCatalog.SalesOutId,
                KpiName = "Principal Sales-Out",
                TargetKpiId = PrincipalKpiCatalog.TargetId,
                AchievementAmountKpiId = PrincipalKpiCatalog.AchievementAmountId,
                AchievementPercentageKpiId = PrincipalKpiCatalog.AchievementPercentageId,
                Disclosures = PrincipalSalesOutDisclosure.Statements.ToList()
            };

            if (snapshot is null || snapshot.KpiId != PrincipalKpiCatalog.SalesOutId)
                return response;

            var ranking = (snapshot.Principals ?? new List<PrincipalSalesOutRow>())
                .Where(row => row != null && row.KpiId == PrincipalKpiCatalog.SalesOutId)
                .OrderBy(row => row.SortOrder)
                .ThenByDescending(row => row.SalesOutAmount)
                .ThenBy(row => row.SupplierId, StringComparer.OrdinalIgnoreCase)
                .Select((row, index) => new PrincipalPerformanceRankingItem
                {
                    Rank = row.SortOrder > 0 ? row.SortOrder : index + 1,
                    PrincipalName = row.SupplierName ?? string.Empty,
                    SupplierId = row.SupplierId ?? string.Empty,
                    KpiId = PrincipalKpiCatalog.SalesOutId,
                    PrincipalSalesOutAmount = row.SalesOutAmount
                })
                .ToList();

            response.IsAvailable = true;
            response.PeriodYear = snapshot.PeriodYear;
            response.PeriodMonth = snapshot.PeriodMonth;
            response.GeneratedAt = snapshot.GeneratedAt;
            response.PrincipalSalesOutAmount = ranking.Sum(row => row.PrincipalSalesOutAmount);
            response.UnknownPrincipalExceptionCount = CountUnknownPrincipalExceptions(snapshot.DataQuality);
            response.Ranking = ranking;

            AttachStoredTargetAndAchievement(response, snapshot, target, achievement);
            return response;
        }

        private static void AttachStoredTargetAndAchievement(
            PrincipalPerformanceResponse response,
            PrincipalSalesOutAggregateResult snapshot,
            PrincipalTargetAggregateResult target,
            PrincipalAchievementResult achievement)
        {
            var targetsBySupplier = IndexMatchingTargets(target, snapshot.PeriodYear, snapshot.PeriodMonth);
            var achievementsBySupplier = IndexMatchingAchievements(achievement, snapshot.PeriodYear, snapshot.PeriodMonth);

            if (targetsBySupplier.Count > 0)
            {
                response.PrincipalTargetAmount = targetsBySupplier.Values.Sum(row => row.TargetAmount);
            }

            foreach (var item in response.Ranking)
            {
                if (string.IsNullOrWhiteSpace(item.SupplierId))
                    continue;

                if (targetsBySupplier.TryGetValue(item.SupplierId.Trim(), out var targetRow))
                {
                    item.TargetKpiId = PrincipalKpiCatalog.TargetId;
                    item.PrincipalTargetAmount = targetRow.TargetAmount;
                }

                if (achievementsBySupplier.TryGetValue(item.SupplierId.Trim(), out var achievementRow))
                {
                    item.AchievementAmountKpiId = PrincipalKpiCatalog.AchievementAmountId;
                    item.AchievementPercentageKpiId = PrincipalKpiCatalog.AchievementPercentageId;
                    item.AchievementAmount = achievementRow.AchievementAmount;
                    item.AchievementPercentage = achievementRow.AchievementPercentage;
                }
            }

            response.MissingTargetExceptionCount = response.Ranking
                .Count(item => item.PrincipalTargetAmount == null);

            if (targetsBySupplier.Count > 0 || achievementsBySupplier.Count > 0)
            {
                response.TargetAchievementIsAvailable = true;
                response.AchievementAmount = CalculateTotalAchievementAmount(
                    response.PrincipalSalesOutAmount,
                    response.PrincipalTargetAmount);
                response.AchievementPercentage = CalculateTotalAchievementPercentage(
                    response.PrincipalSalesOutAmount,
                    response.PrincipalTargetAmount);
            }
        }

        private static Dictionary<string, PrincipalTargetRow> IndexMatchingTargets(
            PrincipalTargetAggregateResult target,
            int periodYear,
            int periodMonth)
        {
            var targetsBySupplier = new Dictionary<string, PrincipalTargetRow>(StringComparer.OrdinalIgnoreCase);
            if (target is null)
                return targetsBySupplier;

            if (target.KpiId != PrincipalKpiCatalog.TargetId)
                return targetsBySupplier;

            if (target.PeriodYear != periodYear || target.PeriodMonth != periodMonth)
                return targetsBySupplier;

            foreach (var row in target.Principals ?? new List<PrincipalTargetRow>())
            {
                if (row is null)
                    continue;

                if (!string.Equals(row.KpiId, PrincipalKpiCatalog.TargetId, StringComparison.Ordinal))
                    continue;

                var supplierId = (row.SupplierId ?? string.Empty).Trim();
                if (supplierId.Length == 0 || targetsBySupplier.ContainsKey(supplierId))
                    continue;

                targetsBySupplier[supplierId] = row;
            }

            return targetsBySupplier;
        }

        private static Dictionary<string, PrincipalAchievementRow> IndexMatchingAchievements(
            PrincipalAchievementResult achievement,
            int periodYear,
            int periodMonth)
        {
            var achievementsBySupplier = new Dictionary<string, PrincipalAchievementRow>(StringComparer.OrdinalIgnoreCase);
            if (achievement is null)
                return achievementsBySupplier;

            if (achievement.PeriodYear != periodYear || achievement.PeriodMonth != periodMonth)
                return achievementsBySupplier;

            foreach (var row in achievement.Principals ?? new List<PrincipalAchievementRow>())
            {
                if (row is null)
                    continue;

                var supplierId = (row.SupplierId ?? string.Empty).Trim();
                if (supplierId.Length == 0 || achievementsBySupplier.ContainsKey(supplierId))
                    continue;

                achievementsBySupplier[supplierId] = row;
            }

            return achievementsBySupplier;
        }

        private static decimal? CalculateTotalAchievementAmount(decimal salesOutAmount, decimal? targetAmount)
        {
            if (!targetAmount.HasValue || targetAmount.Value <= 0m)
                return null;

            return salesOutAmount - targetAmount.Value;
        }

        private static decimal? CalculateTotalAchievementPercentage(decimal salesOutAmount, decimal? targetAmount)
        {
            if (!targetAmount.HasValue || targetAmount.Value <= 0m)
                return null;

            return Math.Round(
                salesOutAmount / targetAmount.Value,
                PrincipalAchievementSnapshot.PercentageScale,
                MidpointRounding.AwayFromZero);
        }

        private static int CountUnknownPrincipalExceptions(
            IEnumerable<PrincipalSalesOutDataQualityRow> dataQuality)
        {
            return (dataQuality ?? Enumerable.Empty<PrincipalSalesOutDataQualityRow>())
                .Where(IsUnknownPrincipalException)
                .Sum(row => row.LineCount);
        }

        private static bool IsUnknownPrincipalException(PrincipalSalesOutDataQualityRow row)
        {
            if (row is null || string.IsNullOrWhiteSpace(row.ExceptionCode))
                return false;

            return row.ExceptionCode == PrincipalSalesOutSnapshot.BlankSupplierExceptionCode
                || row.ExceptionCode == PrincipalSalesOutSnapshot.UnknownSupplierExceptionCode;
        }
    }
}
