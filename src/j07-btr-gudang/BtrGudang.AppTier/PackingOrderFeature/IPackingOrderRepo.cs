using BtrGudang.Domain.PackingOrderFeature;
using BtrGudang.Helper.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BtrGudang.AppTier.PackingOrderFeature
{
    public interface IPackingOrderRepo :
        ISaveChange<PackingOrderModel>,
        IDeleteEntity<IPackingOrderKey>,
        ILoadEntity<PackingOrderModel, IPackingOrderKey>
    {
        IEnumerable<PackingOrderView> ListData(Periode periode);
        IEnumerable<PackingOrderView> ListByDownloadTimestamp(DateTime downloadTimestamp);
        PackingOrderView GetLastDownloadByTimestamp();
    }
}
