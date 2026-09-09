using System.Collections.Generic;

namespace btr.application.ReportingContext.PrincipalAnalyticsAgg
{
    /// <summary>
    /// PD-010 statements required on every Principal sales surface.
    /// </summary>
    public static class PrincipalSalesOutDisclosure
    {
        public const string ReturnsDoNotReduceOrRedefinePrincipalSalesOut =
            "Returns do not reduce or redefine Principal Sales-Out.";

        public static readonly IReadOnlyList<string> Statements = new[]
        {
            "The measure is Principal Sales-Out (DPP) from Faktur Item.",
            ReturnsDoNotReduceOrRedefinePrincipalSalesOut,
            "Returns, Claims, and Inventory Adjustments are not deducted.",
            "Returns are independent KPIs and do not redefine Sales-Out.",
            "Tax and header totals are excluded.",
            "Totals are not required to reconcile to Faktur GrandTotal.",
            "Item Principal comes from current Item master and is treated as immutable for analytics.",
            "Historical periods use the best available data and may contain documented limitations.",
            "Unknown Principal and missing monthly target responsibility are visible exceptions, not silent drops of Principal Sales-Out."
        };
    }
}
