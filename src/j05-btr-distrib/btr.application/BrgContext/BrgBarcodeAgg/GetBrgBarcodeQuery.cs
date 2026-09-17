using System.Threading;
using System.Threading.Tasks;
using btr.domain.BrgContext.BrgBarcodeAgg;
using Dawn;
using MediatR;

namespace btr.application.BrgContext.BrgBarcodeAgg
{
    public class GetBrgBarcodeQuery : IRequest<BrgBarcodeModel>, IBrgBarcodeKey
    {
        public GetBrgBarcodeQuery(string brgBarcodeId) => BrgBarcodeId = brgBarcodeId;
        public string BrgBarcodeId { get; }
    }

    public class GetBrgBarcodeHandler : IRequestHandler<GetBrgBarcodeQuery, BrgBarcodeModel>
    {
        private BrgBarcodeModel _aggRoot = new BrgBarcodeModel();
        private readonly IBrgBarcodeBuilder _builder;

        public GetBrgBarcodeHandler(IBrgBarcodeBuilder builder)
        {
            _builder = builder;
        }

        public Task<BrgBarcodeModel> Handle(GetBrgBarcodeQuery request, CancellationToken cancellationToken)
        {
            //  GUARD
            Guard.Argument(() => request).NotNull()
                .Member(x => x.BrgBarcodeId, y => y.NotEmpty());

            //  QUERY
            _aggRoot = _builder
                .Load(request)
                .Build();

            //  RESPONSE
            return Task.FromResult(_aggRoot);
        }
    }
}
