using btr.nuna.Domain;
using btr.nuna.Infrastructure;
using System.Collections.Generic;

namespace btr.application.SalesContext.FakturBrgInfo
{
    public interface IFakturBrgViewDal 
        : IListData<FakturBrgView, Periode>
    {
        IEnumerable<FakturBrgView> ListTerhapus(Periode periode);
    }
}
