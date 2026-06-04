using BtrGudang.AppTier.PackingOrderFeature;
using BtrGudang.Domain.PackingOrderFeature;
using BtrGudang.Helper.Common;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BtrGudang.Infrastructure.PackingOrderFeature
{
    public class PackingOrderRepo : IPackingOrderRepo
    {
        private readonly IPackingOrderDal _packingOrderDal;
        private readonly IPackingOrderItemDal _packingOrderItemDal;

        public PackingOrderRepo(IPackingOrderDal packingOrderDal, 
            IPackingOrderItemDal packingOrderItemDal)
        {
            _packingOrderDal = packingOrderDal;
            _packingOrderItemDal = packingOrderItemDal;
        }


        public void SaveChanges(PackingOrderModel model)
        {
            LoadEntity(model)
                .Match(
                    onSome: _ => _packingOrderDal.Update(PackingOrderDto.FromModel(model)),
                    onNone: () => _packingOrderDal.Insert(PackingOrderDto.FromModel(model)));


            _packingOrderItemDal.Delete(model);
            _packingOrderItemDal.Insert(model.ListItem
                .Select(x => PackingOrderItemDto.FromModel(x, model.PackingOrderId))
                .ToList());
        }

        public void DeleteEntity(IPackingOrderKey key)
        {
            _packingOrderDal.Delete(key);
            _packingOrderItemDal.Delete(key);
        }

        public MayBe<PackingOrderModel> LoadEntity(IPackingOrderKey key)
        {
            var hdr = _packingOrderDal.GetData(key);
            var listDtl = _packingOrderItemDal.ListData(key).SafeToList();
            var listDtlModel = listDtl
                .OrderBy(x => x.NoUrut)
                .Select(x => x.ToModel())
                .ToList();
            var model = hdr?.ToModel(listDtlModel);
            return MayBe.From(model);
        }

        public IEnumerable<PackingOrderView> ListData(Periode periode)
        {
            return _packingOrderDal.ListDataView(periode);
        }

        public IEnumerable<PackingOrderView> ListByDownloadTimestamp(DateTime downloadTimestamp)
        {
            return _packingOrderDal.ListByDownloadTimestamp(downloadTimestamp);
        }

        public PackingOrderView GetLastDownloadByTimestamp()
        {
            return _packingOrderDal.GetLastDownloadByTimestamp();
        }
    }
}
