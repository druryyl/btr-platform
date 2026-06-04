using btrade.domain.SalesFeature;
using Nuna.Lib.DataAccessHelper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace btrade.application.Contract
{
    public interface IOrderItemDal :
        IInsertBulk<OrderItemType>,
        IDelete<IOrderKey>,
        IListData<OrderItemType, IOrderKey>
    {
    }
}
