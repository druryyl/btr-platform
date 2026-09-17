using btrade.domain.ReturnOrderFeature;
using Nuna.Lib.DataAccessHelper;

namespace btrade.application.Contract
{
    public interface IReturnOrderItemDal :
        IInsertBulk<ReturnOrderItemType>,
        IDelete<IReturnOrderKey>,
        IListData<ReturnOrderItemType, IReturnOrderKey>
    {
    }
}
