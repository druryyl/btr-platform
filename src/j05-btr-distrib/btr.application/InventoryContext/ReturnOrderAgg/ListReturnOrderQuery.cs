using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using btr.application.InventoryContext.ReturnOrderAgg.Contracts;
using btr.application.InventoryContext.WarehouseAgg;
using btr.domain.InventoryContext.ReturnOrderAgg;
using btr.domain.InventoryContext.WarehouseAgg;
using btr.nuna.Application;
using btr.nuna.Domain;
using Dawn;
using MediatR;

namespace btr.application.InventoryContext.ReturnOrderAgg
{
    public class ListReturnOrderQuery : IRequest<IEnumerable<ReturnOrderModel>>
    {
        public ListReturnOrderQuery(string tgl1, string tgl2, string customerId = "")
        {
            Tgl1 = tgl1;
            Tgl2 = tgl2;
            CustomerId = customerId ?? string.Empty;
        }

        public string Tgl1 { get; }
        public string Tgl2 { get; }
        public string CustomerId { get; }
    }

    public class ListReturnOrderHandler
        : IRequestHandler<ListReturnOrderQuery, IEnumerable<ReturnOrderModel>>
    {
        private const string SyncedStatus = "Synced";

        private readonly IReturnOrderDal _returnOrderDal;
        private readonly IWarehouseDal _warehouseDal;

        public ListReturnOrderHandler(IReturnOrderDal returnOrderDal,
            IWarehouseDal warehouseDal)
        {
            _returnOrderDal = returnOrderDal;
            _warehouseDal = warehouseDal;
        }

        public Task<IEnumerable<ReturnOrderModel>> Handle(ListReturnOrderQuery request,
            CancellationToken cancellationToken)
        {
            //  GUARD
            Guard.Argument(() => request).NotNull()
                .Member(x => x.Tgl1, y => y.ValidDate("yyyy-MM-dd"))
                .Member(x => x.Tgl2, y => y.ValidDate("yyyy-MM-dd"));

            var tgl1 = request.Tgl1.ToDate(DateFormatEnum.YMD);
            var tgl2 = request.Tgl2.ToDate(DateFormatEnum.YMD);

            //  QUERY — the Generate worklist carries Synced orders only; an
            //  Imported order is consumed and never listed again (INV-10,
            //  ADR-RO-006). The DAL already orders by ReturnOrderDate DESC,
            //  ReturnOrderId DESC (§7.1 worklist order).
            var listOrder = _returnOrderDal.ListByStatus(SyncedStatus)
                            ?? Enumerable.Empty<ReturnOrderModel>();

            var result = listOrder
                .Where(x => x.ReturnOrderDate.Date >= tgl1.Date &&
                            x.ReturnOrderDate.Date <= tgl2.Date)
                .Where(x => string.IsNullOrWhiteSpace(request.CustomerId) ||
                            x.CustomerId == request.CustomerId)
                .ToList();

            ResolveWarehouseName(result);

            //  RESPONSE
            return Task.FromResult(result.AsEnumerable());
        }

        private void ResolveWarehouseName(IEnumerable<ReturnOrderModel> listOrder)
        {
            //  §7.1 — WarehouseName is a read convenience; the DAL joins the
            //  Customer/SalesPerson/Driver names but not the warehouse master.
            foreach (var order in listOrder)
            {
                if (string.IsNullOrWhiteSpace(order.WarehouseCode))
                    continue;

                order.WarehouseName = _warehouseDal
                    .GetData(new WarehouseModel(order.WarehouseCode))?.WarehouseName
                    ?? string.Empty;
            }
        }
    }
}
