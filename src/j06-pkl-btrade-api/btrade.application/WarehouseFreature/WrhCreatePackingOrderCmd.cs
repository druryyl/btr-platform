using btrade.domain.WarehouseFeature;
using MediatR;
using Nuna.Lib.TransactionHelper;
using Nuna.Lib.ValidationHelper;

namespace btrade.application.WarehouseFreature
{
    public record WrhSavePackingOrderCmd(
        string PackingOrderId, string PackingOrderDate, 
        string CustomerId, string CustomerCode, string CustomerName, string Alamat, string NoTelp,
        double Latitude, double Longitude, double Accuracy,
        string FakturId, string FakturCode, string FakturDate, string AdminName, decimal GrandTotal,
        string DriverId, string DriverName, string OfficeCode, string Note,
        IEnumerable<WrhSavePackingOrderItemCmd> ListItem) : IRequest, IPackingOrderKey;
    
    public record WrhSavePackingOrderItemCmd(
        int NoUrut, string BrgId, string BrgCode, 
        string BrgNme, string KategoriName, string SupplierName,
        int QtyBesar, string SatBesar,
        int QtyKecil, string SatKecil, string DepoId);

    public class WrhSavePackingOrderCmdHandler : IRequestHandler<WrhSavePackingOrderCmd>
    {
        private readonly IPackingOrderDal _packingOrderDal;
        private readonly IPackingOrderItemDal _packingOrderItemDal;
        private readonly IPackingOrderDepoDal _packingOrderDepoDal;

        public WrhSavePackingOrderCmdHandler(IPackingOrderDal packingOrderDal, IPackingOrderItemDal packingOrderItemDal, 
            IPackingOrderDepoDal packingOrderDepoDal)
        {
            _packingOrderDal = packingOrderDal;
            _packingOrderItemDal = packingOrderItemDal;
            _packingOrderDepoDal = packingOrderDepoDal;
        }

        public Task Handle(WrhSavePackingOrderCmd req, CancellationToken cancellationToken)
        {
            var listItem = req.ListItem.Select(item => new PackingOrderItemModel(
                req.PackingOrderId,
                item.NoUrut, item.BrgId, item.BrgNme, item.BrgCode, item.KategoriName, item.SupplierName,
                item.QtyBesar, item.SatBesar,
                item.QtyKecil, item.SatKecil, item.DepoId)).ToList();

            var listDepo = listItem
                .GroupBy(x => x.DepoId)
                .Select(g => new PackingOrderDepoModel(req.PackingOrderId, g.Key, DateTime.Now, new DateTime(3000,1,1)))
                .ToList();

            var timestampNow = DateTime.Now;
            var packingOrder = new PackingOrderModel(
                req.PackingOrderId, req.PackingOrderDate.ToDate(DateFormatEnum.YMD_HMS), 
                req.CustomerId, req.CustomerCode, req.CustomerName, req.Alamat, req.NoTelp,
                req.Latitude, req.Longitude, req.Accuracy,
                req.FakturId, req.FakturCode, req.FakturDate.ToDate(DateFormatEnum.YMD_HMS), req.AdminName, req.GrandTotal,
                req.DriverId, req.DriverName,
                req.OfficeCode, req.Note, listItem, listDepo);

            var packingOrderDb = _packingOrderDal.GetData(req);
            using (var trans = TransHelper.NewScope())
            {

                if (packingOrderDb is not null)
                    _packingOrderDal.Update(packingOrder);
                else
                    _packingOrderDal.Insert(packingOrder);

                _packingOrderItemDal.Delete(req);
                _packingOrderItemDal.Insert(listItem);

                _packingOrderDepoDal.Delete(req);
                _packingOrderDepoDal.Insert(listDepo);
                trans.Complete();
            }

            return Task.CompletedTask;
        }
    }
}
