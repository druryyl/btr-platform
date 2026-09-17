using System.Threading;
using System.Threading.Tasks;
using btr.domain.BrgContext.BrgAgg;
using btr.domain.BrgContext.BrgBarcodeAgg;
using btr.nuna.Domain;
using Dawn;
using MediatR;

namespace btr.application.BrgContext.BrgBarcodeAgg
{
    public class CorrectBrgBarcodeCommand : IRequest<CorrectBrgBarcodeResponse>, IBrgBarcodeKey
    {
        public CorrectBrgBarcodeCommand(string brgBarcodeId, string brgId,
            string satuan = "", string userId = "")
        {
            BrgBarcodeId = brgBarcodeId;
            BrgId = brgId;
            Satuan = satuan ?? string.Empty;
            UserId = userId ?? string.Empty;
        }

        public string BrgBarcodeId { get; }
        public string BrgId { get; }
        public string Satuan { get; }
        public string UserId { get; }
    }

    public class CorrectBrgBarcodeResponse
    {
        public string BrgBarcodeId { get; set; }
        public string BarcodeValue { get; set; }
        public string BrgId { get; set; }
        public string BrgCode { get; set; }
        public string BrgName { get; set; }
        public string Satuan { get; set; }
        public bool IsAktif { get; set; }
    }

    public class CorrectBrgBarcodeHandler
        : IRequestHandler<CorrectBrgBarcodeCommand, CorrectBrgBarcodeResponse>
    {
        private readonly IBrgBarcodeBuilder _builder;
        private readonly IBrgBarcodeWriter _writer;
        private readonly DateTimeProvider _dateTime;

        public CorrectBrgBarcodeHandler(IBrgBarcodeBuilder builder,
            IBrgBarcodeWriter writer,
            DateTimeProvider dateTime)
        {
            _builder = builder;
            _writer = writer;
            _dateTime = dateTime;
        }

        public Task<CorrectBrgBarcodeResponse> Handle(
            CorrectBrgBarcodeCommand request,
            CancellationToken cancellationToken)
        {
            //  GUARD
            Guard.Argument(() => request).NotNull()
                .Member(x => x.BrgBarcodeId, y => y.NotEmpty())
                .Member(x => x.BrgId, y => y.NotEmpty());

            //  BUILD — changes BrgId/Satuan only; activation state is preserved (INV-07)
            var aggRoot = _builder
                .Load(request)
                .Brg(new BrgModel(request.BrgId))
                .Satuan(request.Satuan)
                .Build();

            //  AUDIT
            aggRoot.ModifiedBy = request.UserId;
            aggRoot.ModifiedDate = _dateTime.Now;

            //  APPLY
            _writer.Save(ref aggRoot);
            return Task.FromResult(GenResponse(aggRoot));
        }

        private static CorrectBrgBarcodeResponse GenResponse(BrgBarcodeModel model)
        {
            return new CorrectBrgBarcodeResponse
            {
                BrgBarcodeId = model.BrgBarcodeId,
                BarcodeValue = model.BarcodeValue,
                BrgId = model.BrgId,
                BrgCode = model.BrgCode,
                BrgName = model.BrgName,
                Satuan = model.Satuan,
                IsAktif = model.IsAktif
            };
        }
    }
}
