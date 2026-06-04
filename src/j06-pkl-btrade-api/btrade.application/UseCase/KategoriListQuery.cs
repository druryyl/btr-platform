using btrade.application.Contract;
using btrade.domain.SalesFeature;
using MediatR;

namespace btrade.application.UseCase
{
    public record KategoriListQuery(string ServerId) : IRequest<IEnumerable<KategoriType>>, IServerId;

    public class KategoriListQueryHandler : IRequestHandler<KategoriListQuery, IEnumerable<KategoriType>>
    {
        private readonly IKategoriDal _kategoriDal;

        public KategoriListQueryHandler(IKategoriDal kategoriDal)
        {
            _kategoriDal = kategoriDal;
        }

        public Task<IEnumerable<KategoriType>> Handle(KategoriListQuery request, CancellationToken cancellationToken)
        {
            var listData = _kategoriDal.ListData(request);
            return Task.FromResult(listData.HasValue
                ? listData.Value
                : Enumerable.Empty<KategoriType>());
        }
    }
}
