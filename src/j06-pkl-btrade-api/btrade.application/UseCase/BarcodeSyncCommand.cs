using btrade.application.Contract;
using btrade.domain.BarcodeFeature;
using btrade.domain.SalesFeature;
using MediatR;
using Nuna.Lib.TransactionHelper;

namespace btrade.application.UseCase;

public record BarcodeSyncCommand(
    IEnumerable<BarcodeType> ListUpsert,
    IEnumerable<string> ListRemove,
    string ServerId) : IRequest<Unit>, IServerId;

public class BarcodeSyncHandler : IRequestHandler<BarcodeSyncCommand, Unit>
{
    private readonly IBarcodeDal _barcodeDal;

    public BarcodeSyncHandler(IBarcodeDal barcodeDal)
    {
        _barcodeDal = barcodeDal;
    }

    public Task<Unit> Handle(BarcodeSyncCommand request, CancellationToken cancellationToken)
    {
        using var trans = TransHelper.NewScope();

        foreach (var item in request.ListUpsert)
        {
            var model = item with { ServerId = request.ServerId };
            var existing = _barcodeDal.GetData(model);
            if (existing.HasValue)
                _barcodeDal.Update(model);
            else
                _barcodeDal.Insert(model);
        }

        foreach (var brgBarcodeId in request.ListRemove)
        {
            _barcodeDal.Delete(new BarcodeKey(brgBarcodeId, request.ServerId));
        }

        trans.Complete();
        return Task.FromResult(Unit.Value);
    }
}

public record BarcodeKey(string BrgBarcodeId, string ServerId) : IBarcodeKey;
