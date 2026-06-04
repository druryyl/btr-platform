using btrade.domain.WarehouseFeature;
using MediatR;

namespace btrade.application.WarehouseFreature
{
    public record WrhDownloadPackingOrderCmd(string StartTimestamp, string depoId, int pageSize)
        : IRequest<WrhDownloadPackingOrderResp>;
    public record WrhDownloadPackingOrderResp(
        string LastTimestamp,
        IEnumerable<WrhDownloadPackingOrderRespHdr> ListData);
    public record WrhDownloadPackingOrderRespHdr(
        string PackingOrderId, string PackingOrderDate,
        string CustomerId, string CustomerCode, string CustomerName, string Alamat, string NoTelp,
        double Latitude, double Longitude, double Accuracy,
        string FakturId, string FakturCode, string FakturDate, string AdminName, decimal GrandTotal,
        string DriverId, string DriverName, string WarehouseDesc, string OfficeCode, string Note,
        IEnumerable<WrhDownloadPackingOrderRespDtl> ListItem);
    public record WrhDownloadPackingOrderRespDtl(
        int NoUrut, string BrgId, string BrgCode, string BrgNme, string KategoriName, string SupplierName,
        int QtyBesar, string SatBesar, int QtyKecil, string SatKecil, string depoId);

    public class WrhDownloadPackingOrderHandler : IRequestHandler<WrhDownloadPackingOrderCmd, WrhDownloadPackingOrderResp>
    {
        private readonly IPackingOrderDal _packingOrderDal;
        private readonly IPackingOrderItemDal _packingOrderItemDal;
        private readonly IPackingOrderDepoDal _packingOrderDepoDal;

        public WrhDownloadPackingOrderHandler(IPackingOrderDal packingOrderDal,
            IPackingOrderItemDal packingItemOrderDal,
            IPackingOrderDepoDal packingOrderDepoDal)
        {
            _packingOrderDal = packingOrderDal;
            _packingOrderItemDal = packingItemOrderDal;
            _packingOrderDepoDal = packingOrderDepoDal;
        }

        public Task<WrhDownloadPackingOrderResp> Handle(WrhDownloadPackingOrderCmd request, CancellationToken cancellationToken)
        {
            var timeStamp = DateTime.Parse(request.StartTimestamp);
            var listHdrView = _packingOrderDal
                .ListData(timeStamp, request.depoId)?.ToList()
                ?? new List<PackingOrderView>();
            listHdrView = listHdrView
                .OrderBy(x => x.UpdateTimestamp)
                .Take(request.pageSize)
                .ToList();

            var listItem = _packingOrderItemDal.ListData(timeStamp, request.depoId)?.ToList()
                ?? new List<PackingOrderItemModel>();
            var listDepo = _packingOrderDepoDal.ListData(timeStamp, request.depoId)?.ToList()
                ?? new List<PackingOrderDepoModel>();

            var listHdr = listHdrView
                .Select(x => new PackingOrderModel(
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
                    listItem.Where(y => y.PackingOrderId == x.PackingOrderId),
                    listDepo.Where(z => z.PackingOrderId == x.PackingOrderId)
                ))
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

            var listDepoToUpdate = listDepo
                .Where(x => x.DepoId == request.depoId)
                .ToList();
            foreach (var depo in listDepoToUpdate)
            {
                _packingOrderDepoDal.Update(depo);
            }

            return Task.FromResult(new WrhDownloadPackingOrderResp(
                listHdrView.Any() ? listHdrView.Max(x => x.UpdateTimestamp).ToString("yyyy-MM-dd HH:mm:ss") : request.StartTimestamp,
                result
            ));
        }
    }
}
