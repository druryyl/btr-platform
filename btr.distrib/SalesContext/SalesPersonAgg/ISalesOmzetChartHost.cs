using System.Collections.Generic;
using btr.application.SalesContext.OrderFeature;
using btr.domain.SalesContext.SalesOmzetAgg;
using btr.nuna.Domain;

namespace btr.distrib.SalesContext.SalesPersonAgg
{
    public interface ISalesOmzetChartHost
    {
        IReadOnlyList<SalesOmzetView> FilteredRows { get; }

        IReadOnlyList<SalesOmzetView> FullRows { get; }

        Periode ReportPeriode { get; }

        SalesOmzetPeriodFilterMode PeriodMode { get; }

        string SearchKeyword { get; }

        bool IsManagerView { get; }

        string GetCurrentUserDisplayName();

        void ApplySalesPersonFilter(string salesPersonName);
    }
}
