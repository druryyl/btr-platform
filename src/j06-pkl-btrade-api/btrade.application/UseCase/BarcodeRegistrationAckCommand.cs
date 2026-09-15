using btrade.application.Contract;
using btrade.domain.BarcodeFeature;
using btrade.domain.SalesFeature;
using MediatR;
using Nuna.Lib.TransactionHelper;

namespace btrade.application.UseCase;

public record BarcodeRegistrationAckCommand(
    string BarcodeRegistrationId,
    string Status,
    string ProcessedNote,
    string ServerId) : IRequest<Unit>, IServerId;

public class BarcodeRegistrationAckCommandHandler
    : IRequestHandler<BarcodeRegistrationAckCommand, Unit>
{
    private readonly IBarcodeRegistrationDal _barcodeRegistrationDal;

    public BarcodeRegistrationAckCommandHandler(IBarcodeRegistrationDal barcodeRegistrationDal)
    {
        _barcodeRegistrationDal = barcodeRegistrationDal;
    }

    public Task<Unit> Handle(BarcodeRegistrationAckCommand request, CancellationToken cancellationToken)
    {
        //  INV-11 — Status is terminal once it leaves PENDING.
        if (request.Status != "ACCEPTED" && request.Status != "REJECTED")
            throw new ArgumentException($"Invalid acknowledgement status ({request.Status})");

        var existing = _barcodeRegistrationDal.GetData(new BarcodeRegistrationKey(request.BarcodeRegistrationId));
        if (!existing.HasValue)
            return Task.FromResult(Unit.Value);

        var model = existing.Value;
        //  P-06 — tenant boundary: only the owning tenant may acknowledge.
        if (model.ServerId != request.ServerId)
            return Task.FromResult(Unit.Value);

        //  INV-11 / 10.6 — repeated acks are no-ops.
        if (model.Status != "PENDING")
            return Task.FromResult(Unit.Value);

        var updated = model with
        {
            Status = request.Status,
            ProcessedAt = DateTime.Now,
            ProcessedNote = request.ProcessedNote ?? string.Empty
        };

        using var trans = TransHelper.NewScope();
        _barcodeRegistrationDal.Update(updated);
        trans.Complete();
        return Task.FromResult(Unit.Value);
    }
}

public record BarcodeRegistrationKey(string BarcodeRegistrationId) : IBarcodeRegistrationRequestKey;
