using btrade.domain.WarehouseFeature;
using Nuna.Lib.DataAccessHelper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace btrade.application.WarehouseFreature
{
    public interface IPackingOrderDepoDal :
        IInsertBulk<PackingOrderDepoModel>,
        IDelete<IPackingOrderKey>,
        IUpdate<PackingOrderDepoModel>,
        IListData<PackingOrderDepoModel, IPackingOrderKey>,
        IListData<PackingOrderDepoModel, DateTime, string>
    {
    }
}
