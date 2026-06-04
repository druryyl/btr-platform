using btrade.application.Contract;
using btrade.domain.SalesFeature;
using MediatR;
using Nuna.Lib.TransactionHelper;

namespace btrade.application.UseCase
{
    public record WilayahSyncCommand(IEnumerable<WilayahType> ListWilayah, string ServerId) : IRequest<Unit>, IServerId;
    public class WilayahSyncHandler : IRequestHandler<WilayahSyncCommand, Unit>
    {
        private readonly IWilayahDal _wilayahDal;

        public WilayahSyncHandler(IWilayahDal wilayahDal)
        {
            _wilayahDal = wilayahDal;
        }

        public Task<Unit> Handle(WilayahSyncCommand request, CancellationToken cancellationToken)
        {
            using var trans = TransHelper.NewScope();

            _wilayahDal.Delete(request);

            foreach (var item in request.ListWilayah)
            {
                _wilayahDal.Insert(item);
            }

            trans.Complete();
            return Task.FromResult(Unit.Value);
        }
    }
}
