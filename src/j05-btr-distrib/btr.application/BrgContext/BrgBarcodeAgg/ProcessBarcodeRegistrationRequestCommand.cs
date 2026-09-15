using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using btr.application.BrgContext.BrgAgg;
using btr.domain.BrgContext.BrgAgg;
using btr.domain.BrgContext.BrgBarcodeAgg;
using btr.nuna.Domain;
using Dawn;
using MediatR;

namespace btr.application.BrgContext.BrgBarcodeAgg
{
    public static class BarcodeRegistrationOutcome
    {
        public const string Accepted = "ACCEPTED";
        public const string Rejected = "REJECTED";
    }

    public static class BarcodeRegistrationRejectReason
    {
        public const string DuplicateBarcode = "DUPLICATE_BARCODE";
        public const string ItemNotFound = "ITEM_NOT_FOUND";
        public const string ItemInactive = "ITEM_INACTIVE";
        public const string InvalidUnit = "INVALID_UNIT";
    }

    public class ProcessBarcodeRegistrationRequestCommand
        : IRequest<ProcessBarcodeRegistrationRequestResponse>
    {
        public ProcessBarcodeRegistrationRequestCommand(string barcodeValue, string brgId,
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

    public class ProcessBarcodeRegistrationRequestResponse
    {
        public string Status { get; set; }
        public string Reason { get; set; }
        public string BrgBarcodeId { get; set; }
        public string BarcodeValue { get; set; }
        public string BrgId { get; set; }
        public string Satuan { get; set; }
    }

    public class ProcessBarcodeRegistrationRequestHandler
        : IRequestHandler<ProcessBarcodeRegistrationRequestCommand,
            ProcessBarcodeRegistrationRequestResponse>
    {
        private readonly IBrgBarcodeDal _brgBarcodeDal;
        private readonly IBrgDal _brgDal;
        private readonly IBrgSatuanDal _brgSatuanDal;
        private readonly IMediator _mediator;

        public ProcessBarcodeRegistrationRequestHandler(IBrgBarcodeDal brgBarcodeDal,
            IBrgDal brgDal,
            IBrgSatuanDal brgSatuanDal,
            IMediator mediator)
        {
            _brgBarcodeDal = brgBarcodeDal;
            _brgDal = brgDal;
            _brgSatuanDal = brgSatuanDal;
            _mediator = mediator;
        }

        public async Task<ProcessBarcodeRegistrationRequestResponse> Handle(
            ProcessBarcodeRegistrationRequestCommand request,
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

            //  INV-02 — uniqueness across all lifecycle states; an existing mapping that
            //  matches the request is an idempotent accept (re-relay safety, §8.2)
            var existing = _brgBarcodeDal.GetByValue(BrgBarcodeModel.BarcodeValueKey(normalizedValue));
            if (existing != null)
            {
                var isSameMapping = existing.BrgId == request.BrgId
                                    && existing.Satuan == request.Satuan;
                return isSameMapping
                    ? Accepted(existing)
                    : Rejected(BarcodeRegistrationRejectReason.DuplicateBarcode, normalizedValue);
            }

            //  BR-004 — the referenced Item must exist
            var brg = _brgDal.GetData(new BrgModel(request.BrgId));
            if (brg is null)
                return Rejected(BarcodeRegistrationRejectReason.ItemNotFound, normalizedValue);

            //  BQ-7 — the referenced Item must still be Active at synchronization
            if (!brg.IsAktif)
                return Rejected(BarcodeRegistrationRejectReason.ItemInactive, normalizedValue);

            //  INV-05 — when Satuan is present it must belong to the referenced Item
            if (!request.Satuan.IsNullOrEmpty())
            {
                var listSatuan = _brgSatuanDal
                    .ListData((IBrgKey)new BrgModel(request.BrgId))?.ToList()
                    ?? new List<BrgSatuanModel>();
                if (!listSatuan.Any(x => x.Satuan == request.Satuan))
                    return Rejected(BarcodeRegistrationRejectReason.InvalidUnit, normalizedValue);
            }

            //  ACCEPT — delegate to the single authority (BR-011, INV-13).
            //  The register command owns the authoritative validation and commits
            //  in a single transaction (P-03), so no partial state is possible.
            var registered = await _mediator.Send(
                new RegisterBrgBarcodeCommand(request.BarcodeValue, request.BrgId,
                    request.Satuan, request.UserId), cancellationToken);

            return Accepted(registered.BrgBarcodeId, registered.BarcodeValue,
                registered.BrgId, registered.Satuan);
        }

        private static ProcessBarcodeRegistrationRequestResponse Accepted(BrgBarcodeModel model)
            => Accepted(model.BrgBarcodeId, model.BarcodeValue, model.BrgId, model.Satuan);

        private static ProcessBarcodeRegistrationRequestResponse Accepted(string brgBarcodeId,
            string barcodeValue, string brgId, string satuan)
        {
            return new ProcessBarcodeRegistrationRequestResponse
            {
                Status = BarcodeRegistrationOutcome.Accepted,
                Reason = string.Empty,
                BrgBarcodeId = brgBarcodeId,
                BarcodeValue = barcodeValue,
                BrgId = brgId,
                Satuan = satuan
            };
        }

        private static ProcessBarcodeRegistrationRequestResponse Rejected(string reason,
            string barcodeValue)
        {
            return new ProcessBarcodeRegistrationRequestResponse
            {
                Status = BarcodeRegistrationOutcome.Rejected,
                Reason = reason,
                BrgBarcodeId = string.Empty,
                BarcodeValue = barcodeValue,
                BrgId = string.Empty,
                Satuan = string.Empty
            };
        }
    }
}
