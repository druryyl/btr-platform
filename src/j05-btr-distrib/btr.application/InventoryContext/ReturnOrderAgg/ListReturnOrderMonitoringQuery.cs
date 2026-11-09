using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using btr.application.InventoryContext.ReturJualAgg.Contracts;
using btr.application.InventoryContext.ReturnOrderAgg.Contracts;
using btr.application.InventoryContext.WarehouseAgg;
using btr.domain.InventoryContext.ReturJualAgg;
using btr.domain.InventoryContext.ReturnOrderAgg;
using btr.nuna.Application;
using btr.nuna.Domain;
using Dawn;
using MediatR;

namespace btr.application.InventoryContext.ReturnOrderAgg
{
    public class ListReturnOrderMonitoringQuery : IRequest<IEnumerable<ListReturnOrderDto>>
    {
        public ListReturnOrderMonitoringQuery(string tgl1, string tgl2)
        {
            Tgl1 = tgl1;
            Tgl2 = tgl2;
        }

        public string Tgl1 { get; }
        public string Tgl2 { get; }
    }

    public class ListReturnOrderMonitoringHandler
        : IRequestHandler<ListReturnOrderMonitoringQuery, IEnumerable<ListReturnOrderDto>>
    {
        private readonly IReturnOrderDal _returnOrderDal;
        private readonly IReturnOrderItemDal _returnOrderItemDal;
        private readonly IReturJualDal _returJualDal;
        private readonly IWarehouseDal _warehouseDal;

        public ListReturnOrderMonitoringHandler(
            IReturnOrderDal returnOrderDal,
            IReturnOrderItemDal returnOrderItemDal,
            IReturJualDal returJualDal,
            IWarehouseDal warehouseDal)
        {
            _returnOrderDal = returnOrderDal;
            _returnOrderItemDal = returnOrderItemDal;
            _returJualDal = returJualDal;
            _warehouseDal = warehouseDal;
        }

        public Task<IEnumerable<ListReturnOrderDto>> Handle(
            ListReturnOrderMonitoringQuery request,
            CancellationToken cancellationToken)
        {
            // 1. GUARD & VALIDATION
            Guard.Argument(() => request).NotNull()
                .Member(x => x.Tgl1, y => y.ValidDate("yyyy-MM-dd"))
                .Member(x => x.Tgl2, y => y.ValidDate("yyyy-MM-dd"));

            var tgl1 = request.Tgl1.ToDate(DateFormatEnum.YMD);
            var tgl2 = request.Tgl2.ToDate(DateFormatEnum.YMD);

            if (tgl1.Date > tgl2.Date)
                throw new ArgumentException("Tanggal awal tidak boleh lebih besar dari tanggal akhir");

            if ((tgl2.Date - tgl1.Date).TotalDays > 31)
                throw new ArgumentException("Rentang tanggal maksimal 31 hari");

            // 2. QUERY RETURN ORDER BY PERIODE
            var periode = new Periode(tgl1, tgl2);
            var listOrder = (_returnOrderDal.ListData(periode) ?? Enumerable.Empty<ReturnOrderModel>()).ToList();

            // Cache warehouse dictionary for lookup
            var warehouseDict = _warehouseDal?.ListData()?
                .Where(w => !string.IsNullOrEmpty(w.WarehouseId))
                .GroupBy(w => w.WarehouseId)
                .ToDictionary(g => g.Key, g => g.First().WarehouseName, StringComparer.OrdinalIgnoreCase)
                ?? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            var result = new List<ListReturnOrderDto>();

            // 3. MAP DTO WITH DETAILS & ASSOCIATED RETUR JUAL
            foreach (var order in listOrder)
            {
                var warehouseName = !string.IsNullOrEmpty(order.WarehouseName)
                    ? order.WarehouseName
                    : (!string.IsNullOrEmpty(order.WarehouseCode) && warehouseDict.TryGetValue(order.WarehouseCode, out var whName)
                        ? whName
                        : string.Empty);

                var items = _returnOrderItemDal.ListData(new ReturnOrderModel(order.ReturnOrderId))
                    ?? Enumerable.Empty<ReturnOrderItemModel>();
                var totalItem = items.Count();

                var returJuals = (_returJualDal.ListByReturnOrderId(order.ReturnOrderId)
                    ?? Enumerable.Empty<ReturJualModel>())
                    .Where(x => !x.IsVoid)
                    .ToList();

                var returJualBagusCode = returJuals
                    .FirstOrDefault(x => string.Equals(x.JenisRetur, "BAGUS", StringComparison.OrdinalIgnoreCase))?
                    .ReturJualCode ?? string.Empty;

                var returJualRusakCode = returJuals
                    .FirstOrDefault(x => string.Equals(x.JenisRetur, "RUSAK", StringComparison.OrdinalIgnoreCase))?
                    .ReturJualCode ?? string.Empty;

                result.Add(new ListReturnOrderDto
                {
                    ReturnOrderId = order.ReturnOrderId ?? string.Empty,
                    ReturnOrderNo = order.ReturnOrderNo ?? string.Empty,
                    ReturnOrderDate = order.ReturnOrderDate,
                    CustomerId = order.CustomerId ?? string.Empty,
                    CustomerName = order.CustomerName ?? string.Empty,
                    WarehouseCode = order.WarehouseCode ?? string.Empty,
                    WarehouseName = warehouseName,
                    SalesPersonId = order.SalesPersonId ?? string.Empty,
                    SalesPersonName = order.SalesPersonName ?? string.Empty,
                    DriverId = order.DriverId ?? string.Empty,
                    DriverName = order.DriverName ?? string.Empty,
                    Status = order.Status ?? string.Empty,
                    TotalItem = totalItem,
                    ReturJualBagusCode = returJualBagusCode,
                    ReturJualRusakCode = returJualRusakCode,
                    Note = order.Note ?? string.Empty
                });
            }

            return Task.FromResult(result.AsEnumerable());
        }
    }
}
