using btrade.domain.WarehouseFeature;
using MediatR;
using Nuna.Lib.TransactionHelper;
using Nuna.Lib.ValidationHelper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace btrade.application.WarehouseFreature
{
    public record WrhBulkUploadPackingOrderCmd(IEnumerable<WrhBulkUploadPackingOrderItemCmd> ListPackingOrder) : IRequest;
    public record WrhBulkUploadPackingOrderItemCmd(
        string PackingOrderId, string PackingOrderDate,
        string CustomerId, string CustomerCode, string CustomerName, string Alamat, string NoTelp,
        double Latitude, double Longitude, double Accuracy,
        string FakturId, string FakturCode, string FakturDate, string AdminName, decimal GrandTotal,
        string DriverId, string DriverName,
        string OfficeCode, string Note,
        IEnumerable<WrhBulkUploadPackingOrderItemBrgCmd> ListItem);

    public record WrhBulkUploadPackingOrderItemBrgCmd(
        int NoUrut, string BrgId, string BrgCode,
        string BrgNme, string KategoriName, string SupplierName,
        int QtyBesar, string SatBesar,
        int QtyKecil, string SatKecil, string DepoId);

    public class WrhBulkUploadPackingOrderHandler : IRequestHandler<WrhBulkUploadPackingOrderCmd>
    {
        private readonly IPackingOrderDal _packingOrderDal;
        private readonly IPackingOrderItemDal _packingOrderItemDal;
        private readonly IPackingOrderDepoDal _packingOrderDepoDal;

        public WrhBulkUploadPackingOrderHandler(IPackingOrderDal packingOrderDal, IPackingOrderItemDal packingOrderItemDal,
            IPackingOrderDepoDal packingOrderDepoDal)
        {
            _packingOrderDal = packingOrderDal;
            _packingOrderItemDal = packingOrderItemDal;
            _packingOrderDepoDal = packingOrderDepoDal;
        }

        public Task Handle(WrhBulkUploadPackingOrderCmd req, CancellationToken cancellationToken)
        {
            using (var trans = TransHelper.NewScope())
            {
                foreach (var hdr in req.ListPackingOrder)
                {
                    var listItem = hdr.ListItem.Select(x => new PackingOrderItemModel(
                        hdr.PackingOrderId,
                        x.NoUrut, x.BrgId, x.BrgCode, x.BrgNme, x.KategoriName, x.SupplierName,
                        x.QtyBesar, x.SatBesar,
                        x.QtyKecil, x.SatKecil, x.DepoId)).ToList();

                    var listDepo = listItem
                        .GroupBy(x => x.DepoId)
                        .Select(g => new PackingOrderDepoModel(hdr.PackingOrderId, g.Key, DateTime.Now, new DateTime(3000, 1, 1)))
                        .ToList();

                    var timestampNow = DateTime.Now;
                    var packingOrder = new PackingOrderModel(
                        hdr.PackingOrderId, hdr.PackingOrderDate.ToDate(DateFormatEnum.YMD_HMS),
                        hdr.CustomerId, hdr.CustomerCode, hdr.CustomerName, hdr.Alamat, hdr.NoTelp,
                        hdr.Latitude, hdr.Longitude, hdr.Accuracy,
                        hdr.FakturId, hdr.FakturCode, hdr.FakturDate.ToDate(DateFormatEnum.YMD_HMS), hdr.AdminName, hdr.GrandTotal,
                        hdr.DriverId, hdr.DriverName,
                        hdr.OfficeCode, hdr.Note, listItem, listDepo);

                    var packingOrderDb = _packingOrderDal.GetData(packingOrder);
                    if (packingOrderDb is not null)
                        _packingOrderDal.Update(packingOrder);
                    else
                        _packingOrderDal.Insert(packingOrder);

                    _packingOrderItemDal.Delete(packingOrder);
                    _packingOrderItemDal.Insert(listItem);

                    _packingOrderDepoDal.Delete(packingOrder);
                    _packingOrderDepoDal.Insert(listDepo);
                }
                trans.Complete();
            }

            return Task.CompletedTask;
        }
    }
}
