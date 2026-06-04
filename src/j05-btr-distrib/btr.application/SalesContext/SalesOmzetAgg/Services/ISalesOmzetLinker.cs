using btr.application.SalesContext.SalesOmzetAgg.Snapshots;
using btr.domain.SalesContext.SalesOmzetAgg;

namespace btr.application.SalesContext.SalesOmzetAgg.Services
{
    public interface ISalesOmzetLinker
    {
        SalesOmzetModel FindOrCreateForOrder(OrderSnapshot order);
        SalesOmzetModel FindOrCreateForFaktur(FakturSnapshot faktur);
        void Refresh(SalesOmzetModel row);
    }
}
