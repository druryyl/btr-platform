using btr.application.ReportingContext.PrincipalAnalyticsAgg.Models;

namespace btr.application.ReportingContext.EntityAnalyticsAgg.Producers
{
    /// <summary>
    /// In-memory Principal KPI aggregates for exactly one replay (closed) month.
    /// Produced by <c>SupplierPrincipalReplayAggregator</c> using the same live
    /// aggregators/composers as the live Principal snapshot workers (formula fidelity).
    /// Consumed only by the <c>SupplierEntityAnalyticsProducer</c> replay branch.
    /// Never persisted directly; L0/L1/L2 persistence stays in the producer.
    /// Null in live contexts (live path keeps reading snapshot DALs).
    /// </summary>
    public sealed class SupplierPrincipalReplayResult
    {
        public int PeriodYear { get; set; }

        public int PeriodMonth { get; set; }

        public PrincipalSalesOutAggregateResult SalesOut { get; set; }

        public PrincipalReturnAggregateResult Returns { get; set; }

        public PrincipalReturnPercentageResult ReturnPercentage { get; set; }

        public PrincipalTargetAggregateResult Targets { get; set; }

        public PrincipalAchievementResult Achievement { get; set; }

        public PrincipalPurchaseInAggregateResult PurchaseIn { get; set; }

        public bool MatchesPeriod(int year, int month)
        {
            return PeriodYear == year && PeriodMonth == month;
        }
    }
}
