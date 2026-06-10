using btr.domain.SalesContext.SalesPersonAgg;
using btr.domain.SalesContext.SalesPersonSupplierAgg;
using btr.nuna.Infrastructure;

namespace btr.application.SalesContext.SalesPersonSupplierAgg.Contracts
{
    public interface ISalesPersonSupplierDal :
        IInsertBulk<SalesPersonSupplierModel>,
        IDelete<ISalesPersonKey>,
        IListData<SalesPersonSupplierModel, ISalesPersonKey>
    {
    }
}
