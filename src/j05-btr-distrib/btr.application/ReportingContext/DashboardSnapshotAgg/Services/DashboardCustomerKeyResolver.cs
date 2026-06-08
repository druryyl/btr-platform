namespace btr.application.ReportingContext.DashboardSnapshotAgg.Services
{
    public static class DashboardCustomerKeyResolver
    {
        public static string ResolveCodeFirst(string customerCode, string customerName)
        {
            if (!string.IsNullOrWhiteSpace(customerCode))
                return customerCode.Trim();

            return customerName?.Trim() ?? string.Empty;
        }
    }
}
