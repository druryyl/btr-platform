using btrade.application.Contract;
using btrade.domain.BarcodeFeature;
using btrade.domain.SalesFeature;
using MediatR;
using Nuna.Lib.TransactionHelper;

namespace btrade.application.UseCase;

public record BarcodeRegistrationSubmitCommand(
    string ClientRequestId,
    string BarcodeValue,
    string BrgId,
    string Satuan,
    string RequestedBy,
    string ServerId) : IRequest<Unit>, IServerId;

public class BarcodeRegistrationSubmitCommandHandler
    : IRequestHandler<BarcodeRegistrationSubmitCommand, Unit>
{
    private readonly IBarcodeRegistrationDal _barcodeRegistrationDal;

    public BarcodeRegistrationSubmitCommandHandler(IBarcodeRegistrationDal barcodeRegistrationDal)
    {
        _barcodeRegistrationDal = barcodeRegistrationDal;
    }

    public Task<Unit> Handle(BarcodeRegistrationSubmitCommand request, CancellationToken cancellationToken)
    {
        //  INV-12 / 10.6 — (ServerId, ClientRequestId) is the idempotency key.
        //  A duplicate submission is acknowledged, not re-staged.
        var existing = _barcodeRegistrationDal.ListData(request);
        if (existing.HasValue &&
            existing.Value.Any(x => x.ClientRequestId == request.ClientRequestId))
            return Task.FromResult(Unit.Value);

        var model = new BarcodeRegistrationRequestType(
            Ulid.NewUlid().ToString(),
            request.ClientRequestId,
            request.ServerId,
            request.BarcodeValue,
            request.BrgId,
            request.Satuan ?? string.Empty,
            request.RequestedBy,
            DateTime.Now,
            "PENDING",
            null,
            string.Empty);

        using var trans = TransHelper.NewScope();
        _barcodeRegistrationDal.Insert(model);
        trans.Complete();
        return Task.FromResult(Unit.Value);
    }
}
