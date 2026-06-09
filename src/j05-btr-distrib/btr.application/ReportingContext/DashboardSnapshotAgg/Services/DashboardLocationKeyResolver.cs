using btr.domain.InventoryContext.WarehouseAgg;

namespace btr.application.ReportingContext.DashboardSnapshotAgg.Services
{
    public static class DashboardLocationKeyResolver
    {
        public const string InTransitWarehouseName = "In-Transit";

        public static string ResolveWarehouseGroupKey(string warehouseName, string warehouseId)
        {
            if (!string.IsNullOrWhiteSpace(warehouseName))
                return warehouseName.Trim();

            if (!string.IsNullOrWhiteSpace(warehouseId))
                return warehouseId.Trim();

            return string.Empty;
        }

        public static string ResolveWarehouseDisplayName(string warehouseName)
        {
            return string.IsNullOrWhiteSpace(warehouseName)
                ? string.Empty
                : warehouseName.Trim();
        }

        public static string ResolveWilayahName(string wilayahName)
        {
            return DashboardInventoryItemGroupBuilder.NormalizeDimensionName(wilayahName);
        }

        public static bool IsInTransitWarehouse(string warehouseName)
        {
            return string.Equals(
                warehouseName?.Trim(),
                InTransitWarehouseName,
                System.StringComparison.Ordinal);
        }

        public static bool IsRankingEligible(WarehouseModel warehouse)
        {
            if (warehouse is null)
                return false;

            if (!warehouse.IsAktif)
                return false;

            if (warehouse.IsSpecial)
                return false;

            return !IsInTransitWarehouse(warehouse.WarehouseName);
        }

        public static bool IsRankingEligible(string warehouseName, bool isAktif, bool isSpecial)
        {
            if (!isAktif || isSpecial)
                return false;

            return !IsInTransitWarehouse(warehouseName);
        }
    }
}
