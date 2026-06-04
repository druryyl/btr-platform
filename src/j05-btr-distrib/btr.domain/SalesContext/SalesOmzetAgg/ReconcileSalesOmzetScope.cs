namespace btr.domain.SalesContext.SalesOmzetAgg
{
    public enum ReconcileSalesOmzetScope
    {
        /// <summary>Process source orders/fakturs in the reconcile periode plus existing aggregate rows overlapping the period.</summary>
        PeriodeScoped = 0,

        /// <summary>Process all source data — not implemented; reserved for Phase 5 scheduler/deploy.</summary>
        Full = 1
    }
}
