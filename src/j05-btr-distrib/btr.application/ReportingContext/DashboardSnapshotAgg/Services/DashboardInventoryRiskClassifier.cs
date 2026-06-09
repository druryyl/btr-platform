using System;
using System.Collections.Generic;
using System.Linq;
using btr.application.SalesContext.FakturInfo;

namespace btr.application.ReportingContext.DashboardSnapshotAgg.Services
{
    public static class DashboardInventoryRiskClassifier
    {
        public static HashSet<string> BuildAtRiskBrgIdSet(
            IEnumerable<DashboardInventoryItemGroup> itemGroups,
            IEnumerable<BrgLastFakturDto> lastFakturRows,
            DateTime today)
        {
            var groups = (itemGroups ?? Enumerable.Empty<DashboardInventoryItemGroup>()).ToList();
            var lastFakturByBrgId = (lastFakturRows ?? Enumerable.Empty<BrgLastFakturDto>())
                .GroupBy(x => x.BrgId ?? string.Empty, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);

            var atRisk = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var item in groups)
            {
                if (IsAtRiskBrgId(item, lastFakturByBrgId, today))
                    atRisk.Add(item.BrgId ?? string.Empty);
            }

            return atRisk;
        }

        public static bool IsAtRiskBrgId(
            DashboardInventoryItemGroup item,
            IReadOnlyDictionary<string, BrgLastFakturDto> lastFakturByBrgId,
            DateTime today)
        {
            if (item is null)
                return false;

            if (!lastFakturByBrgId.TryGetValue(item.BrgId ?? string.Empty, out var lastFaktur))
                return true;

            var idleDays = (today.Date - lastFaktur.LastFakturDate.Date).Days;
            return idleDays >= DashboardInventoryRiskAggregator.SlowMovingDaysThreshold;
        }
    }
}
