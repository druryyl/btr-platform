using BtrGudang.Domain.PackingOrderFeature;
using BtrGudang.Helper.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BtrGudang.AppTier.PackingOrderFeature
{
    public interface IPrintLogRepo :
        ISaveChange<PrintLogType>,
        IDeleteEntity<IPrintLogKey>,
        ILoadEntity<PrintLogType, IPrintLogKey>
    {
    }
}
