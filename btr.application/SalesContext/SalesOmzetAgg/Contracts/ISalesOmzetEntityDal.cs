using System.Collections.Generic;
using btr.domain.SalesContext.SalesOmzetAgg;
using btr.nuna.Domain;
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

        /// <summary>
        /// Existing aggregate rows that may need refresh when source lists alone miss stale data.
        /// Wide net: SalesDate, OmzetDate, OrderDate, or FakturDate in periode (reconcile scope only).
        /// </summary>
        IEnumerable<SalesOmzetModel> ListForReconcileScope(Periode periode);
    }
}
