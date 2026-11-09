using System.Collections.Generic;
using btr.domain.InventoryContext.ReturnOrderAgg;
using btr.nuna.Domain;
using btr.nuna.Infrastructure;

namespace btr.application.InventoryContext.ReturnOrderAgg.Contracts
{
    public interface IReturnOrderDal :
        IInsert<ReturnOrderModel>,
        IUpdate<ReturnOrderModel>,
        IGetData<ReturnOrderModel, IReturnOrderKey>,
        IListData<ReturnOrderModel>,
        IListData<ReturnOrderModel, Periode>
    {
        ReturnOrderModel GetByReturnOrderId(IReturnOrderKey key);
        IEnumerable<ReturnOrderModel> ListByStatus(string status);
        bool Exists(IReturnOrderKey key);
    }
}
