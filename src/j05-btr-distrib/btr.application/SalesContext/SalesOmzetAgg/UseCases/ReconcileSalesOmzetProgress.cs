namespace btr.application.SalesContext.SalesOmzetAgg.UseCases
{
    /// <summary>Progress snapshot for UI during <see cref="IReconcileSalesOmzetWorker.Execute"/>.</summary>
    public class ReconcileSalesOmzetProgress
    {
        public int Current { get; set; }
        public int Total { get; set; }
        public string Phase { get; set; }
    }
}
