using btrade.application.Contract;
using btrade.domain.BarcodeFeature;
using btrade.domain.SalesFeature;
using MediatR;
using Nuna.Lib.TransactionHelper;

namespace btrade.application.UseCase;

public record UserSyncCommand(IEnumerable<UserType> ListUser, string ServerId) : IRequest<Unit>, IServerId;

public class UserSyncHandler : IRequestHandler<UserSyncCommand, Unit>
{
    private readonly IUserDal _userDal;

    public UserSyncHandler(IUserDal userDal)
    {
        _userDal = userDal;
    }

    public Task<Unit> Handle(UserSyncCommand request, CancellationToken cancellationToken)
    {
        using var trans = TransHelper.NewScope();
        _userDal.Delete(request);
        foreach (var item in request.ListUser)
        {
            _userDal.Insert(item with { ServerId = request.ServerId });
        }
        trans.Complete();
        return Task.FromResult(Unit.Value);
    }
}
