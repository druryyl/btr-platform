using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using btr.application.InventoryContext.ReturnOrderAgg.Contracts;
using btr.application.InventoryContext.WarehouseAgg;
using btr.domain.InventoryContext.ReturnOrderAgg;
using btr.domain.InventoryContext.WarehouseAgg;
using btr.domain.SalesContext.CustomerAgg;
using Dawn;
using MediatR;

namespace btr.application.InventoryContext.ReturnOrderAgg
{
    public class ListReturnOrderByCustomerQuery
        : IRequest<IEnumerable<ReturnOrderModel>>, ICustomerKey
    {
        public ListReturnOrderByCustomerQuery(string customerId) => CustomerId = customerId;
        public string CustomerId { get; }
    }

    public class ListReturnOrderByCustomerHandler
        : IRequestHandler<ListReturnOrderByCustomerQuery, IEnumerable<ReturnOrderModel>>
    {
        private const string SyncedStatus = "Synced";

        private readonly IReturnOrderDal _returnOrderDal;
        private readonly IWarehouseDal _warehouseDal;

        public ListReturnOrderByCustomerHandler(IReturnOrderDal returnOrderDal,
            IWarehouseDal warehouseDal)
        {
            _returnOrderDal = returnOrderDal;
            _warehouseDal = warehouseDal;
        }

        public Task<IEnumerable<ReturnOrderModel>> Handle(ListReturnOrderByCustomerQuery request,
            CancellationToken cancellationToken)
        {
            //  GUARD
            Guard.Argument(() => request).NotNull()
                .Member(x => x.CustomerId, y => y.NotEmpty());

            //  QUERY — Customer-scoped listing for the Generate surface; only
            //  Synced orders are selectable (INV-10, ADR-RO-006).
            var listOrder = _returnOrderDal.ListByStatus(SyncedStatus)
                            ?? Enumerable.Empty<ReturnOrderModel>();

            var result = listOrder
                .Where(x => x.CustomerId == request.CustomerId)
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
