using btr.application.SalesContext.SalesOmzetAgg.Snapshots;
using btr.domain.SalesContext.SalesOmzetAgg;

namespace btr.application.SalesContext.SalesOmzetAgg.Policies
{
    public interface ISalesOmzetEligibilityPolicy
    {
        bool IsFakturEligible(FakturSnapshot faktur);
        bool IsOrderEligible(OrderSnapshot order);
        bool ShouldRemove(SalesOmzetModel row, FakturSnapshot linkedFaktur);
    }
}
