using System;
using System.Threading;
using System.Threading.Tasks;
using btr.domain.BrgContext.BrgBarcodeAgg;
using btr.nuna.Domain;
using Dawn;
using MediatR;

namespace btr.application.BrgContext.BrgBarcodeAgg
{
    public class DeactivateBrgBarcodeCommand : IRequest, IBrgBarcodeKey
    {
        public DeactivateBrgBarcodeCommand(string brgBarcodeId, string userId = "")
        {
            BrgBarcodeId = brgBarcodeId;
            UserId = userId ?? string.Empty;
        }

        public string BrgBarcodeId { get; }
        public string UserId { get; }
    }

    public class DeactivateBrgBarcodeHandler : IRequestHandler<DeactivateBrgBarcodeCommand>
    {
        private readonly IBrgBarcodeBuilder _builder;
        private readonly IBrgBarcodeWriter _writer;
        private readonly DateTimeProvider _dateTime;

        public DeactivateBrgBarcodeHandler(IBrgBarcodeBuilder builder,
            IBrgBarcodeWriter writer,
            DateTimeProvider dateTime)
        {
            _builder = builder;
            _writer = writer;
            _dateTime = dateTime;
        }

        public Task Handle(DeactivateBrgBarcodeCommand request,
            CancellationToken cancellationToken)
        {
            //  GUARD
            Guard.Argument(() => request).NotNull()
                .Member(x => x.BrgBarcodeId, y => y.NotEmpty());

            //  LOAD
            var current = _builder.Load(request).Build();
            if (!current.IsAktif)
                throw new ArgumentException($"Barcode already inactive ({current.BarcodeValue})");

            //  APPLY — Active -> Inactive (INV-08); never deleted (BR-006)
            var aggRoot = _builder.Attach(current).Deactivate().Build();
            aggRoot.ModifiedBy = request.UserId;
            aggRoot.ModifiedDate = _dateTime.Now;

            _writer.Save(ref aggRoot);
            return Task.FromResult(Unit.Value);
        }
    }
}
