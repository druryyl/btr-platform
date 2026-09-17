using btr.domain.InventoryContext.ReturnOrderAgg;
using btr.nuna.Infrastructure;

namespace btr.application.InventoryContext.ReturnOrderAgg.Contracts
{
    public interface IReturnOrderItemDal :
        IInsertBulk<ReturnOrderItemModel>,
        IDelete<IReturnOrderKey>,
        IListData<ReturnOrderItemModel, IReturnOrderKey>
    {
    }
}
