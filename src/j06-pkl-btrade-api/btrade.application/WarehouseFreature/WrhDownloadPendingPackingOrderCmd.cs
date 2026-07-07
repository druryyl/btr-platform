using btrade.domain.WarehouseFeature;
using MediatR;

namespace btrade.application.WarehouseFreature
{
    public record WrhDownloadPendingPackingOrderCmd(string depoId, int pageSize)
        : IRequest<WrhDownloadPendingPackingOrderResp>;
    public record WrhDownloadPendingPackingOrderResp(
        IEnumerable<WrhDownloadPackingOrderRespHdr> ListData);

    public class WrhDownloadPendingPackingOrderHandler : IRequestHandler<WrhDownloadPendingPackingOrderCmd, WrhDownloadPendingPackingOrderResp>
    {
        private readonly IPackingOrderDal _packingOrderDal;
        private readonly IPackingOrderItemDal _packingOrderItemDal;
        private readonly IPackingOrderDepoDal _packingOrderDepoDal;

        public WrhDownloadPendingPackingOrderHandler(IPackingOrderDal packingOrderDal,
            IPackingOrderItemDal packingItemOrderDal,
            IPackingOrderDepoDal packingOrderDepoDal)
        {
            _packingOrderDal = packingOrderDal;
            _packingOrderItemDal = packingItemOrderDal;
            _packingOrderDepoDal = packingOrderDepoDal;
        }

        public Task<WrhDownloadPendingPackingOrderResp> Handle(WrhDownloadPendingPackingOrderCmd request, CancellationToken cancellationToken)
        {
            var listHdrView = _packingOrderDal
                .ListPendingData(request.depoId, request.pageSize)?.ToList()
                ?? new List<PackingOrderView>();

            var listHdr = listHdrView
                .Select(x =>
                {
                    var items = _packingOrderItemDal.ListData(x)?.ToList()
                        ?? new List<PackingOrderItemModel>();
                    var depos = _packingOrderDepoDal.ListData(x)?.ToList()
                        ?? new List<PackingOrderDepoModel>();

                    return new PackingOrderModel(
                        x.PackingOrderId,
                        x.PackingOrderDate,
                        x.CustomerId,
                        x.CustomerCode,
                        x.CustomerName,
                        x.Alamat,
                        x.NoTelp,
                        x.Latitude,
                        x.Longitude,
                        x.Accuracy,
                        x.FakturId,
                        x.FakturCode,
                        x.FakturDate,
                        x.AdminName,
                        x.GrandTotal,
                        x.DriverId,
                        x.DriverName,
                        x.OfficeCode,
                        x.Note,
                        items,
                        depos
                    );
                })
                .ToList();

            foreach (var hdr in listHdr)
            {
                hdr.SetDownloadTimestamp(DateTime.Now, request.depoId);
            }

            var result = listHdr
                .Select(x => new WrhDownloadPackingOrderRespHdr(
                    x.PackingOrderId,
                    x.PackingOrderDate.ToString("yyyy-MM-dd HH:mm:ss"),
                    x.CustomerId,
                    x.CustomerCode,
                    x.CustomerName,
                    x.Alamat,
                    x.NoTelp,
                    x.Latitude,
                    x.Longitude,
                    x.Accuracy,
                    x.FakturId,
                    x.FakturCode,
                    x.FakturDate.ToString("yyyy-MM-dd HH:mm:ss"),
                    x.AdminName,
                    x.GrandTotal,
                    x.DriverId,
                    x.DriverName,
                    x.WarehouseDesc,
                    x.OfficeCode,
                    x.Note,
                    x.ListItem.Select(i => new WrhDownloadPackingOrderRespDtl(
                        i.NoUrut,
                        i.BrgId,
                        i.BrgCode,
                        i.BrgName,
                        i.KategoriName,
                        i.SupplierName,
                        i.QtyBesar,
                        i.SatBesar,
                        i.QtyKecil,
                        i.SatKecil,
                        i.DepoId
                    ))
                ));

            foreach (var hdr in listHdr)
            {
                var depo = hdr.ListDepo.FirstOrDefault(d => d.DepoId == request.depoId);
                if (depo != null)
                {
                    _packingOrderDepoDal.Update(depo);
                }
            }

            return Task.FromResult(new WrhDownloadPendingPackingOrderResp(result));
        }
    }
}
