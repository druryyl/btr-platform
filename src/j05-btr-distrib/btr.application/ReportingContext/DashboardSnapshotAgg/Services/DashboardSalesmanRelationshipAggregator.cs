using System;
using System.Collections.Generic;
using System.Linq;
using btr.application.ReportingContext.DashboardSnapshotAgg.Contracts;
using btr.application.ReportingContext.DashboardSnapshotAgg.Models;

namespace btr.application.ReportingContext.DashboardSnapshotAgg.Services
{
    public class DashboardSalesmanRelationshipAggregator
    {
        public const int TopCustomerCount = 10;
        public const int TopItemCount = 10;

        public DashboardSalesmanRelationshipAggregateResult Aggregate(
            IEnumerable<SalesmanMtdItemRollupDto> rollupRows,
            DateTime businessDate,
            DateTime generatedAt)
        {
            var today = businessDate.Date;
            var rows = (rollupRows ?? Enumerable.Empty<SalesmanMtdItemRollupDto>())
                .Where(r => r != null && !string.IsNullOrWhiteSpace(r.SalesPersonId))
                .ToList();

            var bySalesPerson = new Dictionary<string, DashboardSalesmanRelationshipSalesmanRollup>(
                StringComparer.OrdinalIgnoreCase);

            foreach (var group in rows.GroupBy(r => r.SalesPersonId.Trim(), StringComparer.OrdinalIgnoreCase))
            {
                var salesPersonId = group.Key;
                var first = group.First();
                bySalesPerson[salesPersonId] = new DashboardSalesmanRelationshipSalesmanRollup
                {
                    SalesPersonId = salesPersonId,
                    SalesPersonCode = first.SalesPersonCode?.Trim() ?? string.Empty,
                    TopCustomers = BuildTopCustomers(group),
                    TopItems = BuildTopItems(group)
                };
            }

            return new DashboardSalesmanRelationshipAggregateResult
            {
                GeneratedAt = generatedAt,
                BusinessDate = today,
                PeriodYear = today.Year,
                PeriodMonth = today.Month,
                BySalesPersonId = bySalesPerson
            };
        }

        private static List<DashboardSalesmanRelationshipCustomerRow> BuildTopCustomers(
            IEnumerable<SalesmanMtdItemRollupDto> rows)
        {
            return rows
                .Where(r => !string.IsNullOrWhiteSpace(r.CustomerCode))
                .GroupBy(r => r.CustomerCode.Trim(), StringComparer.OrdinalIgnoreCase)
                .Select(g => new
                {
                    CustomerCode = g.Key,
                    Total = g.Sum(x => x.LineTotal)
                })
                .OrderByDescending(x => x.Total)
                .ThenBy(x => x.CustomerCode, StringComparer.OrdinalIgnoreCase)
                .Take(TopCustomerCount)
                .Select((x, index) => new DashboardSalesmanRelationshipCustomerRow
                {
                    Rank = index + 1,
                    CustomerCode = x.CustomerCode,
                    CustomerName = x.CustomerCode,
                    MetricValue = x.Total
                })
                .ToList();
        }

        private static List<DashboardSalesmanRelationshipItemRow> BuildTopItems(
            IEnumerable<SalesmanMtdItemRollupDto> rows)
        {
            return rows
                .Where(r => !string.IsNullOrWhiteSpace(r.BrgId))
                .GroupBy(r => r.BrgId.Trim(), StringComparer.OrdinalIgnoreCase)
                .Select(g =>
                {
                    var first = g.First();
                    return new
                    {
                        BrgId = g.Key,
                        BrgCode = first.BrgCode?.Trim() ?? g.Key,
                        BrgName = first.BrgName?.Trim() ?? g.Key,
                        Total = g.Sum(x => x.LineTotal)
                    };
                })
                .OrderByDescending(x => x.Total)
                .ThenBy(x => x.BrgName, StringComparer.OrdinalIgnoreCase)
                .Take(TopItemCount)
                .Select((x, index) => new DashboardSalesmanRelationshipItemRow
                {
                    Rank = index + 1,
                    BrgId = x.BrgId,
                    BrgCode = x.BrgCode,
                    BrgName = x.BrgName,
                    MetricValue = x.Total
                })
                .ToList();
        }
    }
}
