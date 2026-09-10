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

    public class SupportingRankingOption
    {
        public string KpiId { get; set; }

        public string KpiName { get; set; }
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

        public string MomGrowthKpiId { get; set; }

        public string YoyGrowthKpiId { get; set; }

        public decimal? MomGrowthPercentage { get; set; }

        public decimal? YoyGrowthPercentage { get; set; }

        public bool GrowthIsAvailable { get; set; }

        public bool ContributionIsAvailable { get; set; }

        public IList<string> Disclosures { get; set; }
            = new List<string>();

        public IList<PrincipalPerformanceRankingItem> Ranking { get; set; }
            = new List<PrincipalPerformanceRankingItem>();

        public IList<SupportingRankingOption> SupportingRankingOptions { get; set; }
            = new List<SupportingRankingOption>();

        public IList<PrincipalSalesmanContributionItem> SalesmanContributions { get; set; }
            = new List<PrincipalSalesmanContributionItem>();
    }

    public class PrincipalSalesmanContributionItem
    {
        public string SupplierId { get; set; }

        public string PrincipalName { get; set; }

        public string SalesPersonId { get; set; }

        public string SalesPersonCode { get; set; }

        public string SalesPersonName { get; set; }

        public string SourceSalesOutKpiId { get; set; }

        public decimal ContributionAmount { get; set; }

        public int LineCount { get; set; }

        public bool HasTargetResponsibility { get; set; }
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

        public string MomGrowthKpiId { get; set; }

        public string YoyGrowthKpiId { get; set; }

        public decimal? MomGrowthPercentage { get; set; }

        public decimal? YoyGrowthPercentage { get; set; }
    }

    public class GetPrincipalPerformanceHandler
        : IRequestHandler<GetPrincipalPerformanceQuery, PrincipalPerformanceResponse>
    {
        private readonly IPrincipalSalesOutSnapshotDal _snapshotDal;
        private readonly IPrincipalTargetSnapshotDal _targetSnapshotDal;
        private readonly IPrincipalAchievementSnapshotDal _achievementSnapshotDal;
        private readonly IPrincipalReturnSnapshotDal _returnSnapshotDal;
        private readonly IPrincipalReturnPercentageSnapshotDal _returnPercentageSnapshotDal;
        private readonly IPrincipalMomGrowthSnapshotDal _momGrowthSnapshotDal;
        private readonly IPrincipalYoyGrowthSnapshotDal _yoyGrowthSnapshotDal;
        private readonly IPrincipalSalesmanContributionSnapshotDal _contributionSnapshotDal;

        public GetPrincipalPerformanceHandler(
            IPrincipalSalesOutSnapshotDal snapshotDal,
            IPrincipalTargetSnapshotDal targetSnapshotDal,
            IPrincipalAchievementSnapshotDal achievementSnapshotDal,
            IPrincipalReturnSnapshotDal returnSnapshotDal,
            IPrincipalReturnPercentageSnapshotDal returnPercentageSnapshotDal,
            IPrincipalMomGrowthSnapshotDal momGrowthSnapshotDal,
            IPrincipalYoyGrowthSnapshotDal yoyGrowthSnapshotDal,
            IPrincipalSalesmanContributionSnapshotDal contributionSnapshotDal)
        {
            _snapshotDal = snapshotDal;
            _targetSnapshotDal = targetSnapshotDal;
            _achievementSnapshotDal = achievementSnapshotDal;
            _returnSnapshotDal = returnSnapshotDal;
            _returnPercentageSnapshotDal = returnPercentageSnapshotDal;
            _momGrowthSnapshotDal = momGrowthSnapshotDal;
            _yoyGrowthSnapshotDal = yoyGrowthSnapshotDal;
            _contributionSnapshotDal = contributionSnapshotDal;
        }

        public Task<PrincipalPerformanceResponse> Handle(
            GetPrincipalPerformanceQuery request,
            CancellationToken cancellationToken)
        {
            var contribution = _contributionSnapshotDal is null
                ? null
                : _contributionSnapshotDal.GetCurrent();
            return Task.FromResult(PrincipalPerformanceComposer.Compose(
                _snapshotDal.GetCurrent(),
                _targetSnapshotDal.GetCurrent(),
                _achievementSnapshotDal.GetCurrent(),
                _returnSnapshotDal.GetCurrent(),
                _returnPercentageSnapshotDal.GetCurrent(),
                _momGrowthSnapshotDal is null ? null : _momGrowthSnapshotDal.GetCurrent(),
                _yoyGrowthSnapshotDal is null ? null : _yoyGrowthSnapshotDal.GetCurrent(),
                contribution));
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
            return Compose(snapshot, target, achievement, returns, returnPercentage, null);
        }

        public static PrincipalPerformanceResponse Compose(
            PrincipalSalesOutAggregateResult snapshot,
            PrincipalTargetAggregateResult target,
            PrincipalAchievementResult achievement,
            PrincipalReturnAggregateResult returns,
            PrincipalReturnPercentageResult returnPercentage,
            PrincipalSalesmanContributionResult contribution)
        {
            return Compose(snapshot, target, achievement, returns, returnPercentage, null, null, contribution);
        }

        public static PrincipalPerformanceResponse Compose(
            PrincipalSalesOutAggregateResult snapshot,
            PrincipalTargetAggregateResult target,
            PrincipalAchievementResult achievement,
            PrincipalReturnAggregateResult returns,
            PrincipalReturnPercentageResult returnPercentage,
            PrincipalMomGrowthResult momGrowth,
            PrincipalYoyGrowthResult yoyGrowth,
            PrincipalSalesmanContributionResult contribution)
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
                MomGrowthKpiId = PrincipalKpiCatalog.MomGrowthId,
                YoyGrowthKpiId = PrincipalKpiCatalog.YoyGrowthId,
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
            AttachStoredGrowth(response, snapshot, momGrowth, yoyGrowth);
            AttachStoredSalesmanContributions(response, snapshot, contribution);
            AttachSupportingRankingOptions(response);
            return response;
        }

        private static void AttachStoredGrowth(
            PrincipalPerformanceResponse response,
            PrincipalSalesOutAggregateResult snapshot,
            PrincipalMomGrowthResult momGrowth,
            PrincipalYoyGrowthResult yoyGrowth)
        {
            var momBySupplier = IndexMatchingMomGrowth(momGrowth, snapshot.PeriodYear, snapshot.PeriodMonth);
            var yoyBySupplier = IndexMatchingYoyGrowth(yoyGrowth, snapshot.PeriodYear, snapshot.PeriodMonth);

            if (momBySupplier.Count == 0 && yoyBySupplier.Count == 0)
                return;

            response.GrowthIsAvailable = true;

            foreach (var item in response.Ranking)
            {
                if (string.IsNullOrWhiteSpace(item.SupplierId))
                    continue;

                var key = item.SupplierId.Trim();

                if (momBySupplier.TryGetValue(key, out var momRow))
                {
                    item.MomGrowthKpiId = PrincipalKpiCatalog.MomGrowthId;
                    item.MomGrowthPercentage = momRow.MomGrowthPercentage;
                }

                if (yoyBySupplier.TryGetValue(key, out var yoyRow))
                {
                    item.YoyGrowthKpiId = PrincipalKpiCatalog.YoyGrowthId;
                    item.YoyGrowthPercentage = yoyRow.YoyGrowthPercentage;
                }
            }

            if (momBySupplier.Count > 0)
            {
                response.MomGrowthKpiId = PrincipalKpiCatalog.MomGrowthId;
                response.MomGrowthPercentage = CalculateTotalGrowthPercentage(
                    momBySupplier.Values, row => row.CurrentSalesOutAmount, row => row.PriorSalesOutAmount);
            }

            if (yoyBySupplier.Count > 0)
            {
                response.YoyGrowthKpiId = PrincipalKpiCatalog.YoyGrowthId;
                response.YoyGrowthPercentage = CalculateTotalGrowthPercentage(
                    yoyBySupplier.Values, row => row.CurrentSalesOutAmount, row => row.PriorSalesOutAmount);
            }
        }

        private static Dictionary<string, PrincipalMomGrowthRow> IndexMatchingMomGrowth(
            PrincipalMomGrowthResult momGrowth,
            int periodYear,
            int periodMonth)
        {
            var bySupplier = new Dictionary<string, PrincipalMomGrowthRow>(StringComparer.OrdinalIgnoreCase);
            if (momGrowth is null)
                return bySupplier;

            if (!string.Equals(momGrowth.MomGrowthKpiId, PrincipalKpiCatalog.MomGrowthId, StringComparison.Ordinal))
                return bySupplier;

            if (momGrowth.PeriodYear != periodYear || momGrowth.PeriodMonth != periodMonth)
                return bySupplier;

            foreach (var row in momGrowth.Principals ?? new List<PrincipalMomGrowthRow>())
            {
                if (row is null)
                    continue;

                var supplierId = (row.SupplierId ?? string.Empty).Trim();
                if (supplierId.Length == 0 || bySupplier.ContainsKey(supplierId))
                    continue;

                bySupplier[supplierId] = row;
            }

            return bySupplier;
        }

        private static Dictionary<string, PrincipalYoyGrowthRow> IndexMatchingYoyGrowth(
            PrincipalYoyGrowthResult yoyGrowth,
            int periodYear,
            int periodMonth)
        {
            var bySupplier = new Dictionary<string, PrincipalYoyGrowthRow>(StringComparer.OrdinalIgnoreCase);
            if (yoyGrowth is null)
                return bySupplier;

            if (!string.Equals(yoyGrowth.YoyGrowthKpiId, PrincipalKpiCatalog.YoyGrowthId, StringComparison.Ordinal))
                return bySupplier;

            if (yoyGrowth.PeriodYear != periodYear || yoyGrowth.PeriodMonth != periodMonth)
                return bySupplier;

            foreach (var row in yoyGrowth.Principals ?? new List<PrincipalYoyGrowthRow>())
            {
                if (row is null)
                    continue;

                var supplierId = (row.SupplierId ?? string.Empty).Trim();
                if (supplierId.Length == 0 || bySupplier.ContainsKey(supplierId))
                    continue;

                bySupplier[supplierId] = row;
            }

            return bySupplier;
        }

        private static decimal? CalculateTotalGrowthPercentage<T>(
            IEnumerable<T> rows,
            Func<T, decimal?> currentSelector,
            Func<T, decimal?> priorSelector)
        {
            decimal currentTotal = 0m;
            decimal priorTotal = 0m;
            var hasAny = false;

            foreach (var row in rows)
            {
                var current = currentSelector(row);
                var prior = priorSelector(row);
                if (!current.HasValue || !prior.HasValue)
                    continue;

                currentTotal += current.Value;
                priorTotal += prior.Value;
                hasAny = true;
            }

            if (!hasAny || priorTotal <= 0m)
                return null;

            return Math.Round(
                (currentTotal - priorTotal) / priorTotal,
                PrincipalMomGrowthSnapshot.PercentageScale,
                MidpointRounding.AwayFromZero);
        }

        private static void AttachStoredSalesmanContributions(
            PrincipalPerformanceResponse response,
            PrincipalSalesOutAggregateResult snapshot,
            PrincipalSalesmanContributionResult contribution)
        {
            if (contribution is null)
                return;

            if (!string.Equals(
                contribution.SourceSalesOutKpiId,
                PrincipalKpiCatalog.SalesOutId,
                StringComparison.Ordinal))
                return;

            if (contribution.PeriodYear != snapshot.PeriodYear
                || contribution.PeriodMonth != snapshot.PeriodMonth)
                return;

            var rankedSuppliers = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var item in response.Ranking)
            {
                if (item is null || string.IsNullOrWhiteSpace(item.SupplierId))
                    continue;

                rankedSuppliers.Add(item.SupplierId.Trim());
            }

            var attached = (contribution.Contributions ?? new List<PrincipalSalesmanContributionRow>())
                .Where(row => row != null
                    && !string.IsNullOrWhiteSpace(row.SupplierId)
                    && rankedSuppliers.Contains(row.SupplierId.Trim())
                    && string.Equals(
                        row.SourceSalesOutKpiId,
                        PrincipalKpiCatalog.SalesOutId,
                        StringComparison.Ordinal))
                .OrderBy(row => row.SupplierId, StringComparer.OrdinalIgnoreCase)
                .ThenByDescending(row => row.ContributionAmount)
                .ThenBy(row => row.SalesPersonId, StringComparer.OrdinalIgnoreCase)
                .Select(row => new PrincipalSalesmanContributionItem
                {
                    SupplierId = (row.SupplierId ?? string.Empty).Trim(),
                    PrincipalName = (row.SupplierName ?? string.Empty).Trim(),
                    SalesPersonId = (row.SalesPersonId ?? string.Empty).Trim(),
                    SalesPersonCode = (row.SalesPersonCode ?? string.Empty).Trim(),
                    SalesPersonName = (row.SalesPersonName ?? string.Empty).Trim(),
                    SourceSalesOutKpiId = PrincipalKpiCatalog.SalesOutId,
                    ContributionAmount = row.ContributionAmount,
                    LineCount = row.LineCount,
                    HasTargetResponsibility = row.HasTargetResponsibility
                })
                .ToList();

            if (attached.Count == 0)
                return;

            response.ContributionIsAvailable = true;
            response.SalesmanContributions = attached;
        }

        private static void AttachSupportingRankingOptions(PrincipalPerformanceResponse response)
        {
            var options = new List<SupportingRankingOption>();

            if (response.ReturnIsAvailable
                && response.Ranking.Any(item => item.ReturnPercentage.HasValue))
            {
                options.Add(new SupportingRankingOption
                {
                    KpiId = PrincipalKpiCatalog.ReturnPercentageId,
                    KpiName = "Return Percentage"
                });
            }

            if (response.TargetAchievementIsAvailable
                && response.Ranking.Any(item => item.AchievementPercentage.HasValue))
            {
                options.Add(new SupportingRankingOption
                {
                    KpiId = PrincipalKpiCatalog.AchievementPercentageId,
                    KpiName = "Achievement Percentage"
                });
            }

            if (response.GrowthIsAvailable
                && response.Ranking.Any(item => item.MomGrowthPercentage.HasValue))
            {
                options.Add(new SupportingRankingOption
                {
                    KpiId = PrincipalKpiCatalog.MomGrowthId,
                    KpiName = "Month-over-Month Growth Percentage"
                });
            }

            if (response.GrowthIsAvailable
                && response.Ranking.Any(item => item.YoyGrowthPercentage.HasValue))
            {
                options.Add(new SupportingRankingOption
                {
                    KpiId = PrincipalKpiCatalog.YoyGrowthId,
                    KpiName = "Year-over-Year Growth Percentage"
                });
            }

            response.SupportingRankingOptions = options;
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
