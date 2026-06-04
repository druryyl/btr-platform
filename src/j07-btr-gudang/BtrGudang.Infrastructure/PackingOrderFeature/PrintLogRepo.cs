using BtrGudang.AppTier.PackingOrderFeature;
using BtrGudang.Domain.PackingOrderFeature;
using BtrGudang.Helper.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BtrGudang.Infrastructure.PackingOrderFeature
{
    public class PrintLogRepo : IPrintLogRepo
    {
        private readonly IPrintLogDal _printLogDal;
        private readonly IPrintLogPackingOrderDal _printLogPackingOrderDal;

        public PrintLogRepo(IPrintLogDal printLogDal, 
            IPrintLogPackingOrderDal printLogPackingOrderDal)
        {
            _printLogDal = printLogDal;
            _printLogPackingOrderDal = printLogPackingOrderDal;
        }

        public void SaveChanges(PrintLogType model)
        {
            LoadEntity(model)
                .Match(
                    onSome: _ => _printLogDal.Update(PrintLogDto.FromModel(model)),
                    onNone: () => _printLogDal.Insert(PrintLogDto.FromModel(model)));


            _printLogPackingOrderDal.Delete(model);
            _printLogPackingOrderDal.Insert(model.ListPackingOrder
                .Select(x => PrintLogPackingOrderDto.FromModel(x, model))
                .ToList());
        }

        public void DeleteEntity(IPrintLogKey key)
        {
            _printLogDal.Delete(key);
            _printLogPackingOrderDal.Delete(key);
        }

        public MayBe<PrintLogType> LoadEntity(IPrintLogKey key)
        {
            var hdr = _printLogDal.GetData(key);
            var listDtl = _printLogPackingOrderDal.ListData(key).SafeToList();
            var listDtlModel = listDtl
                .Select(x => x.ToModel())
                .ToList();
            var model = hdr?.ToModel(listDtlModel);
            return MayBe.From(model);
        }
    }
}
