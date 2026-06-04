using System.Collections.Generic;
using btr.domain.SalesContext.SalesOmzetHealthWeeklyAgg;
using btr.nuna.Domain;

namespace btr.application.SalesContext.SalesOmzetHealthWeeklyAgg.Policies
{
    public interface ISalesOmzetReportHealthResolver
    {
        SalesOmzetReportHealthResult Resolve(
            Periode reportPeriod,
            IReadOnlyList<IsoWeekIdentifier> intersectingWeeks,
            IReadOnlyList<SalesOmzetHealthWeeklyModel> persistedRows);
    }
}
