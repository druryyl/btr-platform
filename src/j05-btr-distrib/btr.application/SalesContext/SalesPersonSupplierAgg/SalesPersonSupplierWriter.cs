using btr.application.SalesContext.SalesPersonSupplierAgg.Contracts;
using btr.domain.SalesContext.SalesPersonAgg;
using btr.domain.SalesContext.SalesPersonSupplierAgg;
using btr.nuna.Application;
using System.Collections.Generic;
using System.Linq;

namespace btr.application.SalesContext.SalesPersonSupplierAgg
{
    public interface ISalesPersonSupplierWriter
    {
        void Save(string salesPersonId, IEnumerable<SalesPersonSupplierModel> assignments);
    }

    public class SalesPersonSupplierWriter : ISalesPersonSupplierWriter
    {
        private readonly ISalesPersonSupplierDal _salesPersonSupplierDal;

        public SalesPersonSupplierWriter(ISalesPersonSupplierDal salesPersonSupplierDal)
        {
            _salesPersonSupplierDal = salesPersonSupplierDal;
        }

        public void Save(string salesPersonId, IEnumerable<SalesPersonSupplierModel> assignments)
        {
            var key = new SalesPersonModel(salesPersonId);
            var list = (assignments ?? Enumerable.Empty<SalesPersonSupplierModel>())
                .Select(x => new SalesPersonSupplierModel
                {
                    SalesPersonId = salesPersonId,
                    SupplierId = x.SupplierId
                })
                .ToList();

            using (var trans = TransHelper.NewScope())
            {
                _salesPersonSupplierDal.Delete(key);
                if (list.Count > 0)
                    _salesPersonSupplierDal.Insert(list);
                trans.Complete();
            }
        }
    }
}
