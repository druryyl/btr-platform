using btr.domain.SalesContext.SalesOmzetAgg;
using btr.nuna.Infrastructure;

namespace btr.application.SalesContext.SalesOmzetAgg.Contracts
{
    public interface ISalesOmzetEntityDal :
        IInsert<SalesOmzetModel>,
        IUpdate<SalesOmzetModel>,
        IGetData<SalesOmzetModel, ISalesOmzetKey>
    {
        SalesOmzetModel GetByOrderId(string orderId);
        SalesOmzetModel GetByFakturId(string fakturId);
    }
}
