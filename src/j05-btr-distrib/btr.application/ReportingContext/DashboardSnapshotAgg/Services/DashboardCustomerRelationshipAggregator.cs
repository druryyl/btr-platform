using System;
using System.Collections.Generic;
using System.Linq;
using btr.application.ReportingContext.DashboardSnapshotAgg.Contracts;
using btr.application.ReportingContext.DashboardSnapshotAgg.Models;

namespace btr.application.ReportingContext.DashboardSnapshotAgg.Services
{
    public class DashboardCustomerRelationshipAggregator
    {
        public const int TopItemCount = 10;
        public const int TopPrincipalCount = 10;

        public DashboardCustomerRelationshipAggregateResult Aggregate(
            IEnumerable<CustomerMtdItemRollupDto> rollupRows,
            DateTime businessDate,
            DateTime generatedAt)
        {
            var today = businessDate.Date;
            var rows = (rollupRows ?? Enumerable.Empty<CustomerMtdItemRollupDto>())
                .Where(r => r != null && !string.IsNullOrWhiteSpace(r.CustomerCode))
                .ToList();

            var byCustomer = new Dictionary<string, DashboardCustomerRelationshipCustomerRollup>(
                StringComparer.OrdinalIgnoreCase);

            foreach (var group in rows.GroupBy(r => r.CustomerCode.Trim(), StringComparer.OrdinalIgnoreCase))
            {
                var customerCode = group.Key;
                var rollup = new DashboardCustomerRelationshipCustomerRollup
                {
                    CustomerCode = customerCode,
                    TopItems = BuildTopItems(group),
                    TopPrincipals = BuildTopPrincipals(group)
                };

                byCustomer[customerCode] = rollup;
            }

            return new DashboardCustomerRelationshipAggregateResult
            {
                GeneratedAt = generatedAt,
                BusinessDate = today,
                PeriodYear = today.Year,
                PeriodMonth = today.Month,
                ByCustomerCode = byCustomer
            };
        }

        private static List<DashboardCustomerRelationshipItemRow> BuildTopItems(
            IEnumerable<CustomerMtdItemRollupDto> rows)
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
                .Select((x, index) => new DashboardCustomerRelationshipItemRow
                {
                    Rank = index + 1,
                    BrgId = x.BrgId,
                    BrgCode = x.BrgCode,
                    BrgName = x.BrgName,
                    MetricValue = x.Total
                })
                .ToList();
        }

        private static List<DashboardCustomerRelationshipPrincipalRow> BuildTopPrincipals(
            IEnumerable<CustomerMtdItemRollupDto> rows)
        {
            return rows
                .Where(r => !string.IsNullOrWhiteSpace(r.SupplierId) || !string.IsNullOrWhiteSpace(r.SupplierName))
                .GroupBy(r => ResolvePrincipalKey(r), StringComparer.OrdinalIgnoreCase)
                .Select(g =>
                {
                    var first = g.First();
                    return new
                    {
                        SupplierId = !string.IsNullOrWhiteSpace(first.SupplierId)
                            ? first.SupplierId.Trim()
                            : g.Key,
                        SupplierCode = first.SupplierCode?.Trim() ?? string.Empty,
                        SupplierName = first.SupplierName?.Trim() ?? g.Key,
                        Total = g.Sum(x => x.LineTotal)
                    };
                })
                .OrderByDescending(x => x.Total)
                .ThenBy(x => x.SupplierName, StringComparer.OrdinalIgnoreCase)
                .Take(TopPrincipalCount)
                .Select((x, index) => new DashboardCustomerRelationshipPrincipalRow
                {
                    Rank = index + 1,
                    SupplierId = x.SupplierId,
                    SupplierCode = !string.IsNullOrWhiteSpace(x.SupplierCode) ? x.SupplierCode : x.SupplierId,
                    SupplierName = x.SupplierName,
                    MetricValue = x.Total
                })
                .ToList();
        }

        private static string ResolvePrincipalKey(CustomerMtdItemRollupDto row)
        {
            if (!string.IsNullOrWhiteSpace(row.SupplierId))
                return row.SupplierId.Trim();

            return row.SupplierName?.Trim() ?? string.Empty;
        }
    }
}
