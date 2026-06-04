using btrade.application.Contract;
using btrade.domain.SalesFeature;
using MediatR;

namespace btrade.application.UseCase
{
    public record WilayahListQuery(string ServerId) : IRequest<IEnumerable<WilayahType>>, IServerId;

    public class WilayahListQueryHandler : IRequestHandler<WilayahListQuery, IEnumerable<WilayahType>>
    {
        private readonly IWilayahDal _wilayahDal;

        public WilayahListQueryHandler(IWilayahDal wilayahDal)
        {
            _wilayahDal = wilayahDal;
        }

        public Task<IEnumerable<WilayahType>> Handle(WilayahListQuery request, CancellationToken cancellationToken)
        {
            var listData = _wilayahDal.ListData(request);
            return Task.FromResult(listData.HasValue
                ? listData.Value
                : Enumerable.Empty<WilayahType>());
        }
    }
}
