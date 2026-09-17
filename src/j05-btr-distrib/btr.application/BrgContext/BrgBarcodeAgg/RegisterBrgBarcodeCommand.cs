using System;
using System.Threading;
using System.Threading.Tasks;
using btr.domain.BrgContext.BrgAgg;
using btr.domain.BrgContext.BrgBarcodeAgg;
using btr.nuna.Domain;
using Dawn;
using MediatR;

namespace btr.application.BrgContext.BrgBarcodeAgg
{
    public class RegisterBrgBarcodeCommand : IRequest<RegisterBrgBarcodeResponse>
    {
        public RegisterBrgBarcodeCommand(string barcodeValue, string brgId,
            string satuan = "", string userId = "")
        {
            BarcodeValue = barcodeValue;
            BrgId = brgId;
            Satuan = satuan ?? string.Empty;
            UserId = userId ?? string.Empty;
        }

        public string BarcodeValue { get; }
        public string BrgId { get; }
        public string Satuan { get; }
        public string UserId { get; }
    }

    public class RegisterBrgBarcodeResponse
    {
        public string BrgBarcodeId { get; set; }
        public string BarcodeValue { get; set; }
        public string BrgId { get; set; }
        public string BrgCode { get; set; }
        public string BrgName { get; set; }
        public string Satuan { get; set; }
        public bool IsAktif { get; set; }
    }

    public class RegisterBrgBarcodeHandler
        : IRequestHandler<RegisterBrgBarcodeCommand, RegisterBrgBarcodeResponse>
    {
        private readonly IBrgBarcodeBuilder _builder;
        private readonly IBrgBarcodeWriter _writer;
        private readonly IBrgBarcodeDal _brgBarcodeDal;
        private readonly DateTimeProvider _dateTime;

        public RegisterBrgBarcodeHandler(IBrgBarcodeBuilder builder,
            IBrgBarcodeWriter writer,
            IBrgBarcodeDal brgBarcodeDal,
            DateTimeProvider dateTime)
        {
            _builder = builder;
            _writer = writer;
            _brgBarcodeDal = brgBarcodeDal;
            _dateTime = dateTime;
        }

        public Task<RegisterBrgBarcodeResponse> Handle(
            RegisterBrgBarcodeCommand request,
            CancellationToken cancellationToken)
        {
            //  GUARD
            Guard.Argument(() => request).NotNull()
                .Member(x => x.BarcodeValue, y => y.NotEmpty())
                .Member(x => x.BrgId, y => y.NotEmpty());

            //  INV-01 — mandatory, non-empty after normalization
            var normalizedValue = BrgBarcodeModel.NormalizeBarcodeValue(request.BarcodeValue);
            if (string.IsNullOrWhiteSpace(normalizedValue))
                throw new ArgumentException("BarcodeValue is required");

            //  INV-02 — uniqueness pre-check across all lifecycle states
            if (_brgBarcodeDal.ExistsByValue(BrgBarcodeModel.BarcodeValueKey(normalizedValue)))
                throw new ArgumentException($"Barcode already registered ({normalizedValue})");

            //  BUILD
            var aggRoot = _builder
                .Create()
                .BarcodeValue(request.BarcodeValue)
                .Brg(new BrgModel(request.BrgId))
                .Satuan(request.Satuan)
                .Activate()
                .Build();

            //  AUDIT
            var now = _dateTime.Now;
            aggRoot.CreatedBy = request.UserId;
            aggRoot.CreatedDate = now;
            aggRoot.ModifiedBy = request.UserId;
            aggRoot.ModifiedDate = now;

            //  APPLY
            _writer.Save(ref aggRoot);
            return Task.FromResult(GenResponse(aggRoot));
        }

        private static RegisterBrgBarcodeResponse GenResponse(BrgBarcodeModel model)
        {
            return new RegisterBrgBarcodeResponse
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
