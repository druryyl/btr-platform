namespace btr.application.ReportingContext.DashboardSnapshotAgg.Services
{
    public static class DashboardCollectionKeyResolver
    {
        public const string UnknownWilayahKey = "Unknown";

        public static string ResolveCustomerKey(string customerCode, string customerName)
        {
            return DashboardCustomerKeyResolver.ResolveCodeFirst(customerCode, customerName);
        }

        public static string ResolveSalesPersonId(string salesPersonId, string salesPersonName)
        {
            return DashboardSalesmanKeyResolver.ResolveSalesPersonId(salesPersonId, salesPersonName);
        }

        public static string ResolveWilayahKey(string wilayahId)
        {
            if (!string.IsNullOrWhiteSpace(wilayahId))
                return wilayahId.Trim();

            return UnknownWilayahKey;
        }

        public static string ResolveWilayahDisplayName(string wilayahId, string wilayahName)
        {
            if (!string.IsNullOrWhiteSpace(wilayahName))
                return wilayahName.Trim();

            if (!string.IsNullOrWhiteSpace(wilayahId))
                return wilayahId.Trim();

            return UnknownWilayahKey;
        }
    }
}
