using System.Collections.Generic;

namespace btr.application.ReportingContext.PrincipalAnalyticsAgg
{
    /// <summary>
    /// Return disclosure statements required on the SA04 returns panel and Return Item evidence.
    /// Wording follows PD-003, GR-001, and the Principal KPI Registry. No new business decision.
    /// </summary>
    public static class PrincipalReturnDisclosure
    {
        public static readonly IReadOnlyList<string> Statements = new[]
        {
            "Returns are independent KPIs and do not reduce or redefine Principal Sales-Out.",
            "Return Percentage is a quality ratio, not a deduction from Sales-Out and not Net Sales.",
            "Evidence grain is Return Item."
        };
    }
}
