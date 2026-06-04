using btrade.domain.SalesFeature;
using Nuna.Lib.DataAccessHelper;

namespace btrade.application.Contract
{
    public interface IKategoriDal :
        IInsert<KategoriType>,
        IUpdate<KategoriType>,
        IDelete<IKategoriKey>,
        IGetDataMayBe<KategoriType, IKategoriKey>,
        IListDataMayBe<KategoriType, IServerId>
    {
        void Delete(IServerId server);
    }
}
