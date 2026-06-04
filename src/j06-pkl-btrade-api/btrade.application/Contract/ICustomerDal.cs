using btrade.domain.SalesFeature;
using Nuna.Lib.DataAccessHelper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace btrade.application.Contract;

public interface ICustomerDal :
    IInsert<CustomerType>,
    IUpdate<CustomerType>,
    IDelete<ICustomerKey>,
    IGetDataMayBe<CustomerType, ICustomerKey>,
    IListDataMayBe<CustomerType, IServerId> 
{
    void Delete(IServerId server);
}
