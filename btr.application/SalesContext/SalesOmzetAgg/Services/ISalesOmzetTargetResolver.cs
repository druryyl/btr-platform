using System.Collections.Generic;
using btr.application.SalesContext.OrderFeature;
using btr.nuna.Domain;

namespace btr.application.SalesContext.SalesOmzetAgg.Services
{
    public interface ISalesOmzetTargetResolver
    {
        /// <summary>
        /// Resolves monthly target for the sales person implied by filtered report data.
        /// Returns null when scope is ambiguous (multiple reps, no match).
        /// </summary>
        decimal? ResolveTarget(
            IEnumerable<SalesOmzetView> filteredRows,
            string searchKeyword,
            Periode periode,
            string currentUserDisplayName);
    }
}
