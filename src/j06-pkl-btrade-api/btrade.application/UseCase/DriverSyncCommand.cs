using btrade.application.Contract;
using btrade.domain.DriverFeature;
using btrade.domain.SalesFeature;
using MediatR;
using Nuna.Lib.TransactionHelper;

namespace btrade.application.UseCase;

public record DriverSyncCommand(IEnumerable<DriverType> ListDriver, string ServerId) : IRequest<Unit>, IServerId;

public class DriverSyncHandler : IRequestHandler<DriverSyncCommand, Unit>
{
    private readonly IDriverDal _driverDal;

    public DriverSyncHandler(IDriverDal driverDal)
    {
        _driverDal = driverDal;
    }

    public Task<Unit> Handle(DriverSyncCommand request, CancellationToken cancellationToken)
    {
        using var trans = TransHelper.NewScope();
        _driverDal.Delete(request);
        foreach (var item in request.ListDriver)
        {
            _driverDal.Insert(item);
        }
        trans.Complete();
        return Task.FromResult(Unit.Value);
    }
}
