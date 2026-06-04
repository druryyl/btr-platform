using btrade.domain.WarehouseFeature;
using Nuna.Lib.DataAccessHelper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace btrade.application.WarehouseFreature
{
    public interface IPackingOrderItemDal :
        IInsertBulk<PackingOrderItemModel>,
        IDelete<IPackingOrderKey>,
        IListData<PackingOrderItemModel, IPackingOrderKey>,
        IListData<PackingOrderItemModel, DateTime, string>
    {
    }
}
