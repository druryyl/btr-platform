using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using btr.application.InventoryContext.ReturnOrderAgg.Contracts;
using btr.application.InventoryContext.ReturnOrderAgg.Workers;
using btr.domain.InventoryContext.ReturnOrderAgg;
using btr.nuna.Application;
using btr.nuna.Domain;
using Dawn;
using FluentValidation;
using MediatR;

namespace btr.application.InventoryContext.ReturnOrderAgg
{
    public class ImportReturnOrderCommand : IRequest<ImportReturnOrderResponse>
    {
        public ImportReturnOrderCommand(string returnOrderId, DateTime returnOrderDate,
            string warehouseCode, string customerId, string salesPersonId, string driverId,
            string note, IEnumerable<ReturnOrderItemModel> listItem, string userId = "")
        {
            ReturnOrderId = returnOrderId;
            ReturnOrderDate = returnOrderDate;
            WarehouseCode = warehouseCode ?? string.Empty;
            CustomerId = customerId ?? string.Empty;
            SalesPersonId = salesPersonId ?? string.Empty;
            DriverId = driverId ?? string.Empty;
            Note = note ?? string.Empty;
            ListItem = listItem ?? new List<ReturnOrderItemModel>();
            UserId = userId ?? string.Empty;
        }

        public string ReturnOrderId { get; }
        public DateTime ReturnOrderDate { get; }
        public string WarehouseCode { get; }
        public string CustomerId { get; }
        public string SalesPersonId { get; }
        public string DriverId { get; }
        public string Note { get; }
        public IEnumerable<ReturnOrderItemModel> ListItem { get; }
        public string UserId { get; }
    }

    public class ImportReturnOrderResponse
    {
        public string ReturnOrderId { get; set; }
        public string ReturnOrderNo { get; set; }
        public string Status { get; set; }
    }

    public class ImportReturnOrderHandler
        : IRequestHandler<ImportReturnOrderCommand, ImportReturnOrderResponse>
    {
        private const string SyncedStatus = "Synced";
        private const string NumberPrefix = "RETO";

        private readonly IReturnOrderDal _returnOrderDal;
        private readonly IReturnOrderWriter _returnOrderWriter;
        private readonly IValidator<ReturnOrderModel> _validator;
        private readonly INunaCounterBL _counter;
        private readonly DateTimeProvider _dateTime;

        public ImportReturnOrderHandler(IReturnOrderDal returnOrderDal,
            IReturnOrderWriter returnOrderWriter,
            IValidator<ReturnOrderModel> validator,
            INunaCounterBL counter,
            DateTimeProvider dateTime)
        {
            _returnOrderDal = returnOrderDal;
            _returnOrderWriter = returnOrderWriter;
            _validator = validator;
            _counter = counter;
            _dateTime = dateTime;
        }

        public Task<ImportReturnOrderResponse> Handle(ImportReturnOrderCommand request,
            CancellationToken cancellationToken)
        {
            //  GUARD
            Guard.Argument(() => request).NotNull()
                .Member(x => x.ReturnOrderId, y => y.NotEmpty());

            var key = new ReturnOrderModel(request.ReturnOrderId);

            //  INV-11 / §10.6 — import is idempotent by ReturnOrderId (`Exists`
            //  check). An order that already carries its office number is an
            //  already-imported order: it is returned unchanged and is never
            //  re-numbered or duplicated.
            if (_returnOrderDal.Exists(key))
            {
                var imported = _returnOrderDal.GetData(key);
                if (imported != null && !string.IsNullOrWhiteSpace(imported.ReturnOrderNo))
                    return Task.FromResult(GenResponse(imported));
            }

            //  BUILD — the relayed header and item lines are preserved as
            //  captured; no small-unit normalization (INV-07, ADR-RO-003).
            var model = new ReturnOrderModel
            {
                ReturnOrderId = request.ReturnOrderId,
                ReturnOrderDate = request.ReturnOrderDate,
                WarehouseCode = request.WarehouseCode,
                CustomerId = request.CustomerId,
                SalesPersonId = request.SalesPersonId,
                DriverId = request.DriverId,
                Note = request.Note,
                Status = SyncedStatus,
                ListItem = request.ListItem.ToList()
            };
            var noUrut = 0;
            foreach (var item in model.ListItem)
            {
                item.ReturnOrderId = model.ReturnOrderId;
                item.NoUrut = ++noUrut;
                item.BrgCode = item.BrgCode ?? string.Empty;
                item.BrgName = item.BrgName ?? string.Empty;
                item.SatId = item.SatId ?? string.Empty;
                item.JenisRetur = item.JenisRetur ?? string.Empty;
            }
            model.RemoveNull();

            //  INV-02 … INV-09 — authoritative validation lives office-side
            _validator.ValidateAndThrow(model);

            //  IR-RO-07 / C-1 — the Main Office assigns the business number via
            //  INunaCounterBL; the ULID ReturnOrderId remains the identity and
            //  idempotency key and is never merged with the number (INV-01).
            model.ReturnOrderNo = _counter.Generate(NumberPrefix, IDFormatEnum.PREFYYMnnnnnC);

            //  AUDIT
            var now = _dateTime.Now;
            model.CreatedBy = request.UserId;
            model.CreatedDate = now;
            model.ModifiedBy = request.UserId;
            model.ModifiedDate = now;

            //  APPLY — header + items commit in one transaction (P-04)
            _returnOrderWriter.Save(model);

            return Task.FromResult(GenResponse(model));
        }

        private static ImportReturnOrderResponse GenResponse(ReturnOrderModel model)
        {
            return new ImportReturnOrderResponse
            {
                ReturnOrderId = model.ReturnOrderId,
                ReturnOrderNo = model.ReturnOrderNo,
                Status = model.Status
            };
        }
    }
}
