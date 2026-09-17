using System;
using System.Threading;
using System.Threading.Tasks;
using btr.application.InventoryContext.ReturnOrderAgg.Workers;
using btr.domain.InventoryContext.DriverAgg;
using btr.domain.InventoryContext.ReturnOrderAgg;
using btr.domain.SalesContext.SalesPersonAgg;
using btr.nuna.Domain;
using Dawn;
using MediatR;

namespace btr.application.InventoryContext.ReturnOrderAgg
{
    public class CompleteReturnOrderCommand : IRequest<CompleteReturnOrderResponse>, IReturnOrderKey
    {
        public CompleteReturnOrderCommand(string returnOrderId,
            string salesPersonId, string driverId, string userId = "")
        {
            ReturnOrderId = returnOrderId;
            SalesPersonId = salesPersonId ?? string.Empty;
            DriverId = driverId ?? string.Empty;
            UserId = userId ?? string.Empty;
        }

        public string ReturnOrderId { get; }
        public string SalesPersonId { get; }
        public string DriverId { get; }
        public string UserId { get; }
    }

    public class CompleteReturnOrderResponse
    {
        public string ReturnOrderId { get; set; }
        public string ReturnOrderNo { get; set; }
        public string SalesPersonId { get; set; }
        public string DriverId { get; set; }
        public string Status { get; set; }
    }

    public class CompleteReturnOrderHandler
        : IRequestHandler<CompleteReturnOrderCommand, CompleteReturnOrderResponse>
    {
        private const string SyncedStatus = "Synced";

        private readonly IReturnOrderBuilder _builder;
        private readonly IReturnOrderWriter _writer;
        private readonly DateTimeProvider _dateTime;

        public CompleteReturnOrderHandler(IReturnOrderBuilder builder,
            IReturnOrderWriter writer,
            DateTimeProvider dateTime)
        {
            _builder = builder;
            _writer = writer;
            _dateTime = dateTime;
        }

        public Task<CompleteReturnOrderResponse> Handle(CompleteReturnOrderCommand request,
            CancellationToken cancellationToken)
        {
            //  GUARD
            Guard.Argument(() => request).NotNull()
                .Member(x => x.ReturnOrderId, y => y.NotEmpty());

            //  LOAD — header + items (items are never altered by this command)
            var aggRoot = _builder.Load(request).Build();

            //  INV-10 / ADR-RO-006 — completion happens while the order is
            //  Synced (not yet consumed by Generate). An Imported order is
            //  already owned by the Main Office and is rejected explicitly.
            if (aggRoot.Status != SyncedStatus)
                throw new InvalidOperationException(
                    $"Return Order {aggRoot.ReturnOrderNo} tidak dapat dilengkapi (status: {aggRoot.Status})");

            //  COMPLETE — only the two optional reference fields are touched
            //  (INV-09, ADR-RO-005); ReturnOrderNo and item lines are untouched.
            if (!string.IsNullOrWhiteSpace(request.SalesPersonId))
                _builder.SalesPerson(new SalesPersonModel(request.SalesPersonId));
            else
            {
                aggRoot.SalesPersonId = string.Empty;
                aggRoot.SalesPersonName = string.Empty;
            }

            if (!string.IsNullOrWhiteSpace(request.DriverId))
                _builder.Driver(new DriverModel(request.DriverId));
            else
            {
                aggRoot.DriverId = string.Empty;
                aggRoot.DriverName = string.Empty;
            }

            //  AUDIT
            aggRoot.ModifiedBy = request.UserId;
            aggRoot.ModifiedDate = _dateTime.Now;

            //  APPLY — header keeps its number and Status; items are re-saved unchanged
            _writer.Save(aggRoot);

            return Task.FromResult(GenResponse(aggRoot));
        }

        private static CompleteReturnOrderResponse GenResponse(ReturnOrderModel model)
        {
            return new CompleteReturnOrderResponse
            {
                ReturnOrderId = model.ReturnOrderId,
                ReturnOrderNo = model.ReturnOrderNo,
                SalesPersonId = model.SalesPersonId,
                DriverId = model.DriverId,
                Status = model.Status
            };
        }
    }
}
