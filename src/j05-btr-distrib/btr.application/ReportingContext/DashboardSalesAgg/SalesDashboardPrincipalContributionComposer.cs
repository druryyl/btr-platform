using System;
using System.Collections.Generic;
using System.Linq;
using btr.application.ReportingContext.DashboardSalesAgg.Queries;
using btr.application.ReportingContext.PrincipalAnalyticsAgg;
using btr.application.ReportingContext.PrincipalAnalyticsAgg.Models;

namespace btr.application.ReportingContext.DashboardSalesAgg
{
    public static class SalesDashboardPrincipalContributionComposer
    {
        public const string CompanyHeaderNoteText =
            "Company sales totals use Faktur header totals and are not Principal Sales-Out.";

        public static DashboardSalesPrincipalContribution Compose(
            PrincipalSalesOutAggregateResult salesOut,
            PrincipalTargetAggregateResult target)
        {
            var contribution = new DashboardSalesPrincipalContribution
            {
                SalesOutKpiId = PrincipalKpiCatalog.SalesOutId,
                TargetKpiId = PrincipalKpiCatalog.TargetId,
                CompanyHeaderNote = CompanyHeaderNoteText,
                Disclosures = PrincipalSalesOutDisclosure.Statements.ToList()
            };

            if (salesOut is null || salesOut.KpiId != PrincipalKpiCatalog.SalesOutId)
                return contribution;

            var ranking = (salesOut.Principals ?? new List<PrincipalSalesOutRow>())
                .Where(row => row != null && row.KpiId == PrincipalKpiCatalog.SalesOutId)
                .OrderByDescending(row => row.SalesOutAmount)
                .ThenBy(row => row.SortOrder)
                .ThenBy(row => row.SupplierId, StringComparer.OrdinalIgnoreCase)
                .Select((row, index) => new DashboardSalesPrincipalContributionItem
                {
                    Rank = index + 1,
                    PrincipalName = row.SupplierName ?? string.Empty,
                    SupplierId = row.SupplierId ?? string.Empty,
                    SalesOutKpiId = PrincipalKpiCatalog.SalesOutId,
                    PrincipalSalesOutAmount = row.SalesOutAmount
                })
                .ToList();

            AttachStoredPrincipalTarget(ranking, salesOut, target);

            contribution.IsAvailable = true;
            contribution.PeriodYear = salesOut.PeriodYear;
            contribution.PeriodMonth = salesOut.PeriodMonth;
            contribution.PrincipalSalesOutAmount = ranking.Sum(row => row.PrincipalSalesOutAmount);
            contribution.PrincipalTargetAmount = SumStoredPrincipalTarget(target, salesOut);
            contribution.Ranking = ranking;
            return contribution;
        }

        private static void AttachStoredPrincipalTarget(
            IList<DashboardSalesPrincipalContributionItem> ranking,
            PrincipalSalesOutAggregateResult salesOut,
            PrincipalTargetAggregateResult target)
        {
            if (!IsMatchingTargetSnapshot(salesOut, target))
                return;

            var targetsBySupplier = (target.Principals ?? new List<PrincipalTargetRow>())
                .Where(row => row != null
                    && row.KpiId == PrincipalKpiCatalog.TargetId
                    && !string.IsNullOrWhiteSpace(row.SupplierId))
                .GroupBy(row => row.SupplierId, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(
                    group => group.Key,
                    group => group.First().TargetAmount,
                    StringComparer.OrdinalIgnoreCase);

            foreach (var item in ranking)
            {
                if (string.IsNullOrWhiteSpace(item.SupplierId))
                    continue;

                if (!targetsBySupplier.TryGetValue(item.SupplierId, out var targetAmount))
                    continue;

                item.TargetKpiId = PrincipalKpiCatalog.TargetId;
                item.PrincipalTargetAmount = targetAmount;
            }
        }

        private static decimal? SumStoredPrincipalTarget(
            PrincipalTargetAggregateResult target,
            PrincipalSalesOutAggregateResult salesOut)
        {
            if (!IsMatchingTargetSnapshot(salesOut, target))
                return null;

            return (target.Principals ?? new List<PrincipalTargetRow>())
                .Where(row => row != null && row.KpiId == PrincipalKpiCatalog.TargetId)
                .Sum(row => row.TargetAmount);
        }

        private static bool IsMatchingTargetSnapshot(
            PrincipalSalesOutAggregateResult salesOut,
            PrincipalTargetAggregateResult target)
        {
            return target != null
                && target.KpiId == PrincipalKpiCatalog.TargetId
                && target.PeriodYear == salesOut.PeriodYear
                && target.PeriodMonth == salesOut.PeriodMonth;
        }
    }
}
