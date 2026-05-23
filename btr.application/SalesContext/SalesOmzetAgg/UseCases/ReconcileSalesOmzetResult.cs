using System;
using btr.domain.SalesContext.SalesOmzetAgg;

namespace btr.application.SalesContext.SalesOmzetAgg.UseCases
{
    /// <summary>Metrics from a single reconcile run (scoped or full).</summary>
    public class ReconcileSalesOmzetResult
    {
        public int OrdersProcessed { get; set; }
        public int FaktursProcessed { get; set; }
        public int RowsRefreshed { get; set; }
        public int RowsCreated { get; set; }
        public TimeSpan Duration { get; set; }
        public ReconcileSalesOmzetScope Scope { get; set; }
    }
}
