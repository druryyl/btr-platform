using System;
using System.Collections.Generic;
using btr.nuna.Infrastructure;

namespace btr.application.InventoryContext.StokBalanceInfo
{
    public interface IStokBalanceViewDal :
        IListData<StokBalanceView>
    {
        IEnumerable<StokBalanceView> ListDataAsOf(DateTime asOfDate);
    }
}
