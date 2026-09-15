using System.Threading;
using System.Threading.Tasks;
using btr.domain.BrgContext.BrgBarcodeAgg;
using Dawn;
using MediatR;

namespace btr.application.BrgContext.BrgBarcodeAgg
{
    public class GetBrgBarcodeByValueQuery : IRequest<BrgBarcodeModel>
    {
        public GetBrgBarcodeByValueQuery(string barcodeValue) => BarcodeValue = barcodeValue;
        public string BarcodeValue { get; }
    }

    public class GetBrgBarcodeByValueHandler : IRequestHandler<GetBrgBarcodeByValueQuery, BrgBarcodeModel>
    {
        private readonly IBrgBarcodeDal _brgBarcodeDal;

        public GetBrgBarcodeByValueHandler(IBrgBarcodeDal brgBarcodeDal)
        {
            _brgBarcodeDal = brgBarcodeDal;
        }

        public Task<BrgBarcodeModel> Handle(GetBrgBarcodeByValueQuery request,
            CancellationToken cancellationToken)
        {
            //  GUARD
            Guard.Argument(() => request).NotNull()
                .Member(x => x.BarcodeValue, y => y.NotEmpty());

            //  QUERY — compare on the normalized key, never the raw value (BQ-5)
            var result = _brgBarcodeDal.GetByValue(BrgBarcodeModel.BarcodeValueKey(request.BarcodeValue));

            //  BR-007 — operational lookup returns only Active mappings
            if (result is null || !result.IsAktif)
                return Task.FromResult<BrgBarcodeModel>(null);

            //  RESPONSE
            return Task.FromResult(result);
        }
    }
}
