using btrade.application.Contract;
using btrade.domain.SalesFeature;
using MediatR;
using Nuna.Lib.TransactionHelper;

namespace btrade.application.UseCase;

public record BrgSyncCommand(IEnumerable<BrgType> ListBrg, string ServerId) : IRequest<Unit>, IServerId;

public class BrgSyncHandler: IRequestHandler<BrgSyncCommand, Unit>
{
    private readonly IBrgDal _brgDal;
    public BrgSyncHandler(IBrgDal brgDal)
    {
        _brgDal = brgDal;
    }
    public Task<Unit> Handle(BrgSyncCommand request, CancellationToken cancellationToken)
    {
        using var trans = TransHelper.NewScope();
        _brgDal.Delete(request);
        foreach(var item in request.ListBrg)
        {
            _brgDal.Insert(item);
        }
        trans.Complete();
        return Task.FromResult(Unit.Value);
    }
}
