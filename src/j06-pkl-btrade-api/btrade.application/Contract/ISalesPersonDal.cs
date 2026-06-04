using btrade.domain.SalesFeature;
using Nuna.Lib.DataAccessHelper;
using Nuna.Lib.PatternHelper;

namespace btrade.application.Contract;

public interface ISalesPersonDal :
    IInsert<SalesPersonType>,
    IUpdate<SalesPersonType>,
    IDelete<ISalesPersonKey>,
    IGetDataMayBe<SalesPersonType, ISalesPersonKey>,
    IListDataMayBe<SalesPersonType, IServerId>
{
    void Delete(IServerId server);
    MayBe<SalesPersonType> GetByEmail(string email);
}
