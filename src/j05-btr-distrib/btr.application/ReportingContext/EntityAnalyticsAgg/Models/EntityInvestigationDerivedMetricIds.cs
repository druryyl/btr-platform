namespace btr.application.ReportingContext.EntityAnalyticsAgg.Models
{
    /// <summary>
    /// Investigation-only derived metric identifiers. These are not KPI IDs, have no registry
    /// entry, and carry no ranking or evidence-route ownership.
    /// </summary>
    public static class EntityInvestigationDerivedMetricIds
    {
        /// <summary>
        /// Purchase-to-Sales-Out Ratio = PRN-PUR-001 / PRN-SALES-001.
        /// </summary>
        public const string PurchaseToSalesOutRatio = "purchase-to-sales-out-ratio";
    }
}
