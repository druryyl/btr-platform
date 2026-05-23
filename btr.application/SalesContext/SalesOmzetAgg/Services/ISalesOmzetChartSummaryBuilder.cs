using System.Collections.Generic;
using btr.application.SalesContext.OrderFeature;
using btr.domain.SalesContext.SalesOmzetAgg;
using btr.nuna.Domain;

namespace btr.application.SalesContext.SalesOmzetAgg.Services
{
    public interface ISalesOmzetChartSummaryBuilder
    {
        SalesOmzetChartSummary Build(
            IEnumerable<SalesOmzetView> rows,
            Periode periode,
            SalesOmzetPeriodFilterMode mode,
            decimal? targetAmount = null);

        /// <summary>Top sales persons by recognized omzet (manager comparison chart).</summary>
        IReadOnlyList<SalesOmzetSalesPersonSlice> BuildManagerComparison(
            IEnumerable<SalesOmzetView> rows,
            int topCount = 15);
    }
}
