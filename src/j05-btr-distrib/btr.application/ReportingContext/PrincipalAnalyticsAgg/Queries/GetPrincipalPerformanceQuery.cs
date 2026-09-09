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

        public string GoodReturnAmountKpiId { get; set; }

        public string BrokenReturnAmountKpiId { get; set; }

        public string TotalReturnAmountKpiId { get; set; }

        public string ReturnPercentageKpiId { get; set; }

        public decimal? GoodReturnAmount { get; set; }

        public decimal? BrokenReturnAmount { get; set; }

        public decimal? TotalReturnAmount { get; set; }

        public decimal? ReturnPercentage { get; set; }

        public bool ReturnIsAvailable { get; set; }

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

        public string GoodReturnAmountKpiId { get; set; }

        public string BrokenReturnAmountKpiId { get; set; }

        public string TotalReturnAmountKpiId { get; set; }

        public string ReturnPercentageKpiId { get; set; }

        public decimal? GoodReturnAmount { get; set; }

        public decimal? BrokenReturnAmount { get; set; }

        public decimal? TotalReturnAmount { get; set; }

        public decimal? ReturnPercentage { get; set; }
    }

    public class GetPrincipalPerformanceHandler
        : IRequestHandler<GetPrincipalPerformanceQuery, PrincipalPerformanceResponse>
    {
        private readonly IPrincipalSalesOutSnapshotDal _snapshotDal;
        private readonly IPrincipalTargetSnapshotDal _targetSnapshotDal;
        private readonly IPrincipalAchievementSnapshotDal _achievementSnapshotDal;
        private readonly IPrincipalReturnSnapshotDal _returnSnapshotDal;
        private readonly IPrincipalReturnPercentageSnapshotDal _returnPercentageSnapshotDal;

        public GetPrincipalPerformanceHandler(
            IPrincipalSalesOutSnapshotDal snapshotDal,
            IPrincipalTargetSnapshotDal targetSnapshotDal,
            IPrincipalAchievementSnapshotDal achievementSnapshotDal,
            IPrincipalReturnSnapshotDal returnSnapshotDal,
            IPrincipalReturnPercentageSnapshotDal returnPercentageSnapshotDal)
        {
            _snapshotDal = snapshotDal;
            _targetSnapshotDal = targetSnapshotDal;
            _achievementSnapshotDal = achievementSnapshotDal;
            _returnSnapshotDal = returnSnapshotDal;
            _returnPercentageSnapshotDal = returnPercentageSnapshotDal;
        }

        public Task<PrincipalPerformanceResponse> Handle(
            GetPrincipalPerformanceQuery request,
            CancellationToken cancellationToken)
        {
            return Task.FromResult(PrincipalPerformanceComposer.Compose(
                _snapshotDal.GetCurrent(),
                _targetSnapshotDal.GetCurrent(),
                _achievementSnapshotDal.GetCurrent(),
                _returnSnapshotDal.GetCurrent(),
                _returnPercentageSnapshotDal.GetCurrent()));
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
            return Compose(snapshot, target, achievement, null, null);
        }

        public static PrincipalPerformanceResponse Compose(
            PrincipalSalesOutAggregateResult snapshot,
            PrincipalTargetAggregateResult target,
            PrincipalAchievementResult achievement,
            PrincipalReturnAggregateResult returns,
            PrincipalReturnPercentageResult returnPercentage)
        {
            var response = new PrincipalPerformanceResponse
            {
                KpiId = PrincipalKpiCatalog.SalesOutId,
                KpiName = "Principal Sales-Out",
                TargetKpiId = PrincipalKpiCatalog.TargetId,
                AchievementAmountKpiId = PrincipalKpiCatalog.AchievementAmountId,
                AchievementPercentageKpiId = PrincipalKpiCatalog.AchievementPercentageId,
                GoodReturnAmountKpiId = PrincipalKpiCatalog.GoodReturnAmountId,
                BrokenReturnAmountKpiId = PrincipalKpiCatalog.BrokenReturnAmountId,
                TotalReturnAmountKpiId = PrincipalKpiCatalog.TotalReturnAmountId,
                ReturnPercentageKpiId = PrincipalKpiCatalog.ReturnPercentageId,
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
            AttachStoredReturns(response, snapshot, returns, returnPercentage);
            return response;
        }

        private static void AttachStoredReturns(
            PrincipalPerformanceResponse response,
            PrincipalSalesOutAggregateResult snapshot,
            PrincipalReturnAggregateResult returns,
            PrincipalReturnPercentageResult returnPercentage)
        {
            var returnsBySupplier = IndexMatchingReturns(returns, snapshot.PeriodYear, snapshot.PeriodMonth);
            var percentagesBySupplier = IndexMatchingReturnPercentages(returnPercentage, snapshot.PeriodYear, snapshot.PeriodMonth);

            if (returnsBySupplier.Count == 0 && percentagesBySupplier.Count == 0)
                return;

            response.ReturnIsAvailable = true;

            if (returnsBySupplier.Count > 0)
            {
                response.GoodReturnAmount = returnsBySupplier.Values.Sum(row => row.GoodReturnAmount);
                response.BrokenReturnAmount = returnsBySupplier.Values.Sum(row => row.BrokenReturnAmount);
                response.TotalReturnAmount = returnsBySupplier.Values.Sum(row => row.TotalReturnAmount);
            }

            foreach (var item in response.Ranking)
            {
                if (string.IsNullOrWhiteSpace(item.SupplierId))
                    continue;

                var key = item.SupplierId.Trim();
                if (returnsBySupplier.TryGetValue(key, out var returnRow))
                {
                    item.GoodReturnAmountKpiId = PrincipalKpiCatalog.GoodReturnAmountId;
                    item.BrokenReturnAmountKpiId = PrincipalKpiCatalog.BrokenReturnAmountId;
                    item.TotalReturnAmountKpiId = PrincipalKpiCatalog.TotalReturnAmountId;
                    item.GoodReturnAmount = returnRow.GoodReturnAmount;
                    item.BrokenReturnAmount = returnRow.BrokenReturnAmount;
                    item.TotalReturnAmount = returnRow.TotalReturnAmount;
                }

                if (percentagesBySupplier.TryGetValue(key, out var percentageRow))
                {
                    item.ReturnPercentageKpiId = PrincipalKpiCatalog.ReturnPercentageId;
                    item.ReturnPercentage = percentageRow.ReturnPercentage;
                }
            }

            var totalPercentage = CalculateTotalReturnPercentage(response.TotalReturnAmount, response.PrincipalSalesOutAmount);
            response.ReturnPercentage = totalPercentage;
        }

        private static Dictionary<string, PrincipalReturnRow> IndexMatchingReturns(
            PrincipalReturnAggregateResult returns,
            int periodYear,
            int periodMonth)
        {
            var returnsBySupplier = new Dictionary<string, PrincipalReturnRow>(StringComparer.OrdinalIgnoreCase);
            if (returns is null)
                return returnsBySupplier;

            if (!string.Equals(returns.TotalReturnKpiId, PrincipalKpiCatalog.TotalReturnAmountId, StringComparison.Ordinal))
                return returnsBySupplier;

            if (returns.PeriodYear != periodYear || returns.PeriodMonth != periodMonth)
                return returnsBySupplier;

            foreach (var row in returns.Principals ?? new List<PrincipalReturnRow>())
            {
                if (row is null)
                    continue;

                var supplierId = (row.SupplierId ?? string.Empty).Trim();
                if (supplierId.Length == 0 || returnsBySupplier.ContainsKey(supplierId))
                    continue;

                returnsBySupplier[supplierId] = row;
            }

            return returnsBySupplier;
        }

        private static Dictionary<string, PrincipalReturnPercentageRow> IndexMatchingReturnPercentages(
            PrincipalReturnPercentageResult returnPercentage,
            int periodYear,
            int periodMonth)
        {
            var percentagesBySupplier = new Dictionary<string, PrincipalReturnPercentageRow>(StringComparer.OrdinalIgnoreCase);
            if (returnPercentage is null)
                return percentagesBySupplier;

            if (!string.Equals(returnPercentage.ReturnPercentageKpiId, PrincipalKpiCatalog.ReturnPercentageId, StringComparison.Ordinal))
                return percentagesBySupplier;

            if (returnPercentage.PeriodYear != periodYear || returnPercentage.PeriodMonth != periodMonth)
                return percentagesBySupplier;

            foreach (var row in returnPercentage.Principals ?? new List<PrincipalReturnPercentageRow>())
            {
                if (row is null)
                    continue;

                var supplierId = (row.SupplierId ?? string.Empty).Trim();
                if (supplierId.Length == 0 || percentagesBySupplier.ContainsKey(supplierId))
                    continue;

                percentagesBySupplier[supplierId] = row;
            }

            return percentagesBySupplier;
        }

        private static decimal? CalculateTotalReturnPercentage(decimal? totalReturnAmount, decimal salesOutAmount)
        {
            if (!totalReturnAmount.HasValue)
                return null;

            if (salesOutAmount <= 0m)
                return null;

            return Math.Round(
                totalReturnAmount.Value / salesOutAmount,
                PrincipalReturnPercentageSnapshot.PercentageScale,
                MidpointRounding.AwayFromZero);
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
