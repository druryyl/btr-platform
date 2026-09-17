using System.Collections.Generic;
using btr.domain.InventoryContext.ReturnOrderAgg;
using btr.nuna.Infrastructure;

namespace btr.application.InventoryContext.ReturnOrderAgg.Contracts
{
    public interface IReturnOrderDal :
        IInsert<ReturnOrderModel>,
        IUpdate<ReturnOrderModel>,
        IGetData<ReturnOrderModel, IReturnOrderKey>,
        IListData<ReturnOrderModel>
    {
        ReturnOrderModel GetByReturnOrderId(IReturnOrderKey key);
        IEnumerable<ReturnOrderModel> ListByStatus(string status);
        bool Exists(IReturnOrderKey key);
    }
}
