using System;
using System.Collections.Generic;
using System.Linq;
using btr.application.ReportingContext.DashboardSnapshotAgg.Contracts;
using btr.application.ReportingContext.DashboardSnapshotAgg.Models;

namespace btr.application.ReportingContext.DashboardSnapshotAgg.Services
{
    public class DashboardItemRelationshipAggregator
    {
        public const int TopCustomerCount = 10;
        public const int TopSalesmanCount = 10;

        public DashboardItemRelationshipAggregateResult Aggregate(
            IEnumerable<SalesmanMtdItemRollupDto> rollupRows,
            DateTime businessDate,
            DateTime generatedAt)
        {
            var today = businessDate.Date;
            var rows = (rollupRows ?? Enumerable.Empty<SalesmanMtdItemRollupDto>())
                .Where(r => r != null && !string.IsNullOrWhiteSpace(r.BrgId))
                .ToList();

            var byBrgId = new Dictionary<string, DashboardItemRelationshipItemRollup>(
                StringComparer.OrdinalIgnoreCase);

            foreach (var group in rows.GroupBy(r => r.BrgId.Trim(), StringComparer.OrdinalIgnoreCase))
            {
                var brgId = group.Key;
                var first = group.First();
                byBrgId[brgId] = new DashboardItemRelationshipItemRollup
                {
                    BrgId = brgId,
                    BrgCode = first.BrgCode?.Trim() ?? brgId,
                    BrgName = first.BrgName?.Trim() ?? brgId,
                    SupplierId = first.SupplierId?.Trim() ?? string.Empty,
                    SupplierCode = first.SupplierCode?.Trim() ?? string.Empty,
                    SupplierName = first.SupplierName?.Trim() ?? string.Empty,
                    TopCustomers = BuildTopCustomers(group),
                    TopSalesmen = BuildTopSalesmen(group)
                };
            }

            return new DashboardItemRelationshipAggregateResult
            {
                GeneratedAt = generatedAt,
                BusinessDate = today,
                PeriodYear = today.Year,
                PeriodMonth = today.Month,
                ByBrgId = byBrgId
            };
        }

        private static List<DashboardItemRelationshipCustomerRow> BuildTopCustomers(
            IEnumerable<SalesmanMtdItemRollupDto> rows)
        {
            return rows
                .Where(r => !string.IsNullOrWhiteSpace(r.CustomerCode))
                .GroupBy(r => r.CustomerCode.Trim(), StringComparer.OrdinalIgnoreCase)
                .Select(g =>
                {
                    var first = g.First();
                    return new
                    {
                        CustomerCode = g.Key,
                        CustomerName = first.CustomerName?.Trim() ?? g.Key,
                        Total = g.Sum(x => x.LineTotal)
                    };
                })
                .OrderByDescending(x => x.Total)
                .ThenBy(x => x.CustomerCode, StringComparer.OrdinalIgnoreCase)
                .Take(TopCustomerCount)
                .Select((x, index) => new DashboardItemRelationshipCustomerRow
                {
                    Rank = index + 1,
                    CustomerCode = x.CustomerCode,
                    CustomerName = x.CustomerName,
                    MetricValue = x.Total
                })
                .ToList();
        }

        private static List<DashboardItemRelationshipSalesmanRow> BuildTopSalesmen(
            IEnumerable<SalesmanMtdItemRollupDto> rows)
        {
            return rows
                .Where(r => !string.IsNullOrWhiteSpace(r.SalesPersonId))
                .GroupBy(r => r.SalesPersonId.Trim(), StringComparer.OrdinalIgnoreCase)
                .Select(g =>
                {
                    var first = g.First();
                    return new
                    {
                        SalesPersonId = g.Key,
                        SalesPersonCode = first.SalesPersonCode?.Trim() ?? g.Key,
                        SalesPersonName = first.SalesPersonName?.Trim() ?? first.SalesPersonCode?.Trim() ?? g.Key,
                        Total = g.Sum(x => x.LineTotal)
                    };
                })
                .OrderByDescending(x => x.Total)
                .ThenBy(x => x.SalesPersonCode, StringComparer.OrdinalIgnoreCase)
                .Take(TopSalesmanCount)
                .Select((x, index) => new DashboardItemRelationshipSalesmanRow
                {
                    Rank = index + 1,
                    SalesPersonId = x.SalesPersonId,
                    SalesPersonCode = x.SalesPersonCode,
                    SalesPersonName = x.SalesPersonName,
                    MetricValue = x.Total
                })
                .ToList();
        }
    }
}
