using btrade.application.Contract;
using btrade.domain.SalesFeature;
using MediatR;
using Nuna.Lib.TransactionHelper;

namespace btrade.application.UseCase
{
    public record KategoriSyncCommand(IEnumerable<KategoriType> ListKategori, string ServerId) : IRequest<Unit>, IServerId;

    public class KategoriSyncHandler : IRequestHandler<KategoriSyncCommand, Unit>
    {
        private readonly IKategoriDal _kategoriDal;

        public KategoriSyncHandler(IKategoriDal kategoriDal)
        {
            _kategoriDal = kategoriDal;
        }

        public Task<Unit> Handle(KategoriSyncCommand request, CancellationToken cancellationToken)
        {
            using var trans = TransHelper.NewScope();

            // Clear existing data
            _kategoriDal.Delete(request);

            // Insert new data
            foreach (var item in request.ListKategori)
            {
                _kategoriDal.Insert(item);
            }

            trans.Complete();
            return Task.FromResult(Unit.Value);
        }
    }
}
