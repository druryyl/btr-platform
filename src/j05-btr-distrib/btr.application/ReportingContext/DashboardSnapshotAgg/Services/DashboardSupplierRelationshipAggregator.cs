using System;
using System.Collections.Generic;
using System.Linq;
using btr.application.ReportingContext.DashboardSnapshotAgg.Contracts;
using btr.application.ReportingContext.DashboardSnapshotAgg.Models;

namespace btr.application.ReportingContext.DashboardSnapshotAgg.Services
{
    public class DashboardSupplierRelationshipAggregator
    {
        public const int TopCustomerCount = 10;
        public const int TopSalesmanCount = 10;
        public const int TopItemCount = 10;

        public DashboardSupplierRelationshipAggregateResult Aggregate(
            IEnumerable<SupplierMtdItemRollupDto> rollupRows,
            DateTime businessDate,
            DateTime generatedAt)
        {
            var today = businessDate.Date;
            var rows = (rollupRows ?? Enumerable.Empty<SupplierMtdItemRollupDto>())
                .Where(r => r != null && !string.IsNullOrWhiteSpace(r.SupplierId))
                .ToList();

            var bySupplier = new Dictionary<string, DashboardSupplierRelationshipSupplierRollup>(
                StringComparer.OrdinalIgnoreCase);

            foreach (var group in rows.GroupBy(r => r.SupplierId.Trim(), StringComparer.OrdinalIgnoreCase))
            {
                var supplierId = group.Key;
                var first = group.First();
                bySupplier[supplierId] = new DashboardSupplierRelationshipSupplierRollup
                {
                    SupplierId = supplierId,
                    SupplierCode = first.SupplierCode?.Trim() ?? supplierId,
                    TopCustomers = BuildTopCustomers(group),
                    TopSalesmen = BuildTopSalesmen(group),
                    TopItems = BuildTopItems(group)
                };
            }

            return new DashboardSupplierRelationshipAggregateResult
            {
                GeneratedAt = generatedAt,
                BusinessDate = today,
                PeriodYear = today.Year,
                PeriodMonth = today.Month,
                BySupplierId = bySupplier
            };
        }

        private static List<DashboardSupplierRelationshipCustomerRow> BuildTopCustomers(
            IEnumerable<SupplierMtdItemRollupDto> rows)
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
                .Select((x, index) => new DashboardSupplierRelationshipCustomerRow
                {
                    Rank = index + 1,
                    CustomerCode = x.CustomerCode,
                    CustomerName = x.CustomerCode,
                    MetricValue = x.Total
                })
                .ToList();
        }

        private static List<DashboardSupplierRelationshipSalesmanRow> BuildTopSalesmen(
            IEnumerable<SupplierMtdItemRollupDto> rows)
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
                        Total = g.Sum(x => x.LineTotal)
                    };
                })
                .OrderByDescending(x => x.Total)
                .ThenBy(x => x.SalesPersonCode, StringComparer.OrdinalIgnoreCase)
                .Take(TopSalesmanCount)
                .Select((x, index) => new DashboardSupplierRelationshipSalesmanRow
                {
                    Rank = index + 1,
                    SalesPersonId = x.SalesPersonId,
                    SalesPersonCode = x.SalesPersonCode,
                    SalesPersonName = x.SalesPersonCode,
                    MetricValue = x.Total
                })
                .ToList();
        }

        private static List<DashboardSupplierRelationshipItemRow> BuildTopItems(
            IEnumerable<SupplierMtdItemRollupDto> rows)
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
                .Select((x, index) => new DashboardSupplierRelationshipItemRow
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
