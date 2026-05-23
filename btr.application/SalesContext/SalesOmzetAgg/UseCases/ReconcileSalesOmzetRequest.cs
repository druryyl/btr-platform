using btr.domain.SalesContext.SalesOmzetAgg;
using btr.nuna.Domain;

namespace btr.application.SalesContext.SalesOmzetAgg.UseCases
{
    public class ReconcileSalesOmzetRequest
    {
        public Periode Periode { get; set; }

        /// <summary>Optional — reserved for future audit; reconcile always sets <see cref="SalesOmzetModel.LastReconciledAt"/>.</summary>
        public string UserId { get; set; }

        public ReconcileSalesOmzetScope Scope { get; set; } = ReconcileSalesOmzetScope.PeriodeScoped;
    }
}
