using btrade.domain.CheckInFeature;
using btrade.domain.SalesFeature;
using Nuna.Lib.DataAccessHelper;
using Nuna.Lib.PatternHelper;
using Nuna.Lib.ValidationHelper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace btrade.application.Contract
{
    public interface ICheckInDal :
        IInsert<CheckInType>,
        IUpdate<CheckInType>,
        IDelete<ICheckInKey>,
        IGetDataMayBe<CheckInType, ICheckInKey>,
        IListData<CheckInType, Periode, IServerId>
    {
    }
}
