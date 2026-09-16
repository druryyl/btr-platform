using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using btr.application.BrgContext.BrgAgg;
using btr.application.InventoryContext.ReturnOrderAgg.Workers;
using btr.application.InventoryContext.ReturJualAgg.Workers;
using btr.application.InventoryContext.WarehouseAgg;
using btr.domain.BrgContext.BrgAgg;
using btr.domain.InventoryContext.DriverAgg;
using btr.domain.InventoryContext.ReturnOrderAgg;
using btr.domain.InventoryContext.ReturJualAgg;
using btr.domain.InventoryContext.WarehouseAgg;
using btr.domain.SalesContext.CustomerAgg;
using btr.domain.SalesContext.SalesPersonAgg;
using btr.nuna.Application;
using btr.nuna.Domain;
using Dawn;
using MediatR;

namespace btr.application.InventoryContext.ReturnOrderAgg
{
    public class GenerateSalesReturnFromReturnOrderCommand
        : IRequest<GenerateSalesReturnFromReturnOrderResponse>
    {
        public GenerateSalesReturnFromReturnOrderCommand(
            IEnumerable<string> listReturnOrderId, string userId = "")
        {
            ListReturnOrderId = listReturnOrderId ?? new List<string>();
            UserId = userId ?? string.Empty;
        }

        public IEnumerable<string> ListReturnOrderId { get; }
        public string UserId { get; }
    }

    public class GenerateSalesReturnFromReturnOrderResponse
    {
        public List<GeneratedSalesReturnModel> ListGenerated { get; set; } =
            new List<GeneratedSalesReturnModel>();
        public List<string> ListConsumedReturnOrderId { get; set; } =
            new List<string>();
    }

    public class GeneratedSalesReturnModel
    {
        public string ReturJualId { get; set; }
        public string ReturJualCode { get; set; }
        public string CustomerId { get; set; }
        public string JenisRetur { get; set; }
        public string SalesPersonId { get; set; }
        public string DriverId { get; set; }
    }

    public class GenerateSalesReturnFromReturnOrderHandler
        : IRequestHandler<GenerateSalesReturnFromReturnOrderCommand,
            GenerateSalesReturnFromReturnOrderResponse>
    {
        private const string SyncedStatus = "Synced";
        private const string ImportedStatus = "Imported";

        //  INV-13 / DOMAIN §15 — the Generate command never performs pricing;
        //  the generated documents carry no price and are completed by the
        //  Office Admin in the existing RT1-Retur Jual flow.
        private const string EmptyPriceInput = "0";

        private readonly IReturnOrderBuilder _returnOrderBuilder;
        private readonly IReturnOrderWriter _returnOrderWriter;
        private readonly IReturJualBuilder _returJualBuilder;
        private readonly IReturJualWriter _returJualWriter;
        private readonly ICreateReturJualItemWorker _createReturJualItemWorker;
        private readonly IBrgBuilder _brgBuilder;
        private readonly IWarehouseDal _warehouseDal;
        private readonly DateTimeProvider _dateTime;

        public GenerateSalesReturnFromReturnOrderHandler(
            IReturnOrderBuilder returnOrderBuilder,
            IReturnOrderWriter returnOrderWriter,
            IReturJualBuilder returJualBuilder,
            IReturJualWriter returJualWriter,
            ICreateReturJualItemWorker createReturJualItemWorker,
            IBrgBuilder brgBuilder,
            IWarehouseDal warehouseDal,
            DateTimeProvider dateTime)
        {
            _returnOrderBuilder = returnOrderBuilder;
            _returnOrderWriter = returnOrderWriter;
            _returJualBuilder = returJualBuilder;
            _returJualWriter = returJualWriter;
            _createReturJualItemWorker = createReturJualItemWorker;
            _brgBuilder = brgBuilder;
            _warehouseDal = warehouseDal;
            _dateTime = dateTime;
        }

        public Task<GenerateSalesReturnFromReturnOrderResponse> Handle(
            GenerateSalesReturnFromReturnOrderCommand request,
            CancellationToken cancellationToken)
        {
            //  GUARD
            Guard.Argument(() => request).NotNull();

            var listReturnOrderId = (request.ListReturnOrderId ?? new List<string>())
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Distinct()
                .ToList();
            if (listReturnOrderId.Count == 0)
                throw new ArgumentException("Pilih minimal satu Return Order");

            //  LOAD — header + items for every selected order
            var listOrder = listReturnOrderId
                .Select(x => _returnOrderBuilder.Load(new ReturnOrderModel(x)).Build())
                .ToList();

            //  INV-10 / §10.6 — only Synced (not yet Imported) orders may be
            //  consumed; an Imported order is owned by the Main Office and
            //  cannot be regenerated.
            foreach (var order in listOrder)
                if (order.Status != SyncedStatus)
                    throw new InvalidOperationException(
                        $"Return Order {order.ReturnOrderNo} tidak dapat digenerate (status: {order.Status})");

            var now = _dateTime.Now;
            var result = new GenerateSalesReturnFromReturnOrderResponse();

            using (var trans = TransHelper.NewScope())
            {
                //  INV-12 / ADR-RO-007 — group selected items by exactly
                //  (CustomerId, JenisRetur, SalesPersonId, DriverId). A single
                //  Return Order may fan out into multiple ReturJual documents.
                var listLine = listOrder
                    .SelectMany(order => (order.ListItem ?? new List<ReturnOrderItemModel>())
                        .Select(item => new ReturnOrderLine(order, item)))
                    .ToList();

                var listGroup = listLine.GroupBy(x => new
                {
                    x.Order.CustomerId,
                    JenisRetur = x.Item.JenisRetur ?? string.Empty,
                    x.Order.SalesPersonId,
                    x.Order.DriverId
                });

                foreach (var group in listGroup)
                {
                    var line = group.First();

                    //  §8.5 — resolve the authoritative BTR_Warehouse.WarehouseId
                    //  from the Return Order's WarehouseCode via the existing
                    //  warehouse master.
                    var warehouse = _warehouseDal.GetData(
                                        new WarehouseModel(line.Order.WarehouseCode))
                        ?? throw new KeyNotFoundException(
                            $"Warehouse tidak ditemukan ({line.Order.WarehouseCode})");

                    _returJualBuilder
                        .Create()
                        .Customer(new CustomerModel(line.Order.CustomerId))
                        .Warehouse(warehouse)
                        .ReturJualDate(now)
                        .JenisRetur(line.Item.JenisRetur)
                        .User(request.UserId);

                    //  INV-09 / ADR-RO-005 — Salesman/Driver are optional and
                    //  only completed when present on the source order.
                    if (!string.IsNullOrWhiteSpace(line.Order.SalesPersonId))
                        _returJualBuilder.SalesPerson(
                            new SalesPersonModel(line.Order.SalesPersonId));
                    if (!string.IsNullOrWhiteSpace(line.Order.DriverId))
                        _returJualBuilder.Driver(
                            new DriverModel(line.Order.DriverId));

                    foreach (var item in group)
                        _returJualBuilder.AddItem(GenReturJualItem(item));

                    var returJual = _returJualWriter.Save(_returJualBuilder.Build());

                    result.ListGenerated.Add(new GeneratedSalesReturnModel
                    {
                        ReturJualId = returJual.ReturJualId,
                        ReturJualCode = returJual.ReturJualCode,
                        CustomerId = returJual.CustomerId,
                        JenisRetur = returJual.JenisRetur,
                        SalesPersonId = returJual.SalesPersonId,
                        DriverId = returJual.DriverId
                    });
                }

                //  INV-10 — consumed orders transition Synced → Imported in the
                //  same transaction as the generated documents.
                foreach (var order in listOrder)
                {
                    order.Status = ImportedStatus;
                    order.ModifiedBy = request.UserId;
                    order.ModifiedDate = now;
                    _returnOrderWriter.Save(order);
                    result.ListConsumedReturnOrderId.Add(order.ReturnOrderId);
                }

                trans.Complete();
            }

            return Task.FromResult(result);
        }

        private ReturJualItemModel GenReturJualItem(ReturnOrderLine line)
        {
            var brg = LoadBrg(line.Item.BrgId);

            //  ADR-RO-003 / INV-07 — the recorded Qty and SatId are handed to
            //  the existing ReturJual item-entry machinery; no small-unit
            //  normalization is performed here. Unit conversion (if any) stays
            //  inside the existing ReturJual item-entry worker.
            return _createReturJualItemWorker.Execute(new CreateReturJualItemRequest(
                line.Item.BrgId,
                line.Order.CustomerId,
                EmptyPriceInput,
                GenQtyInputStr(line.Item.Qty, line.Item.SatId, brg),
                (int)line.Item.Qty,
                line.Item.SatId,
                string.Empty,
                0,
                line.Item.JenisRetur));
        }

        private BrgModel LoadBrg(string brgId)
        {
            if (string.IsNullOrWhiteSpace(brgId))
                return new BrgModel { ListSatuan = new List<BrgSatuanModel>() };

            try
            {
                return _brgBuilder.Load(new BrgModel(brgId)).Build();
            }
            catch (KeyNotFoundException)
            {
                return new BrgModel { ListSatuan = new List<BrgSatuanModel>() };
            }
        }

        private static string GenQtyInputStr(decimal qty, string satId, BrgModel brg)
        {
            var listSatuan = brg?.ListSatuan ?? new List<BrgSatuanModel>();
            var satuanBesar = listSatuan
                .Where(x => x.Conversion > 1)
                .OrderByDescending(x => x.Conversion)
                .FirstOrDefault();

            var qtyStr = qty.ToString("0.##", CultureInfo.InvariantCulture);

            //  The recorded quantity is placed in the slot that matches the
            //  recorded SatId so the unit survives; the value itself is never
            //  converted into a small-unit total.
            if (satuanBesar != null &&
                string.Equals(satId, satuanBesar.Satuan, StringComparison.OrdinalIgnoreCase))
                return $"{qtyStr};0";

            return $"0;{qtyStr}";
        }

        private class ReturnOrderLine
        {
            public ReturnOrderLine(ReturnOrderModel order, ReturnOrderItemModel item)
            {
                Order = order;
                Item = item;
            }

            public ReturnOrderModel Order { get; }
            public ReturnOrderItemModel Item { get; }
        }
    }
}
