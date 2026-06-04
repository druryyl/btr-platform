using btr.nuna.Domain;

namespace btr.application.SalesContext.SalesOmzetHealthWeeklyAgg.Contracts
{
    public interface ISalesOmzetHealthMetricsDal
    {
        SalesOmzetHealthMetrics GetMetrics(Periode week);
    }
}
