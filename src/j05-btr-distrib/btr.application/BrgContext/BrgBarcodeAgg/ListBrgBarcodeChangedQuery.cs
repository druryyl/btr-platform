using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using btr.domain.BrgContext.BrgBarcodeAgg;
using Dawn;
using MediatR;

namespace btr.application.BrgContext.BrgBarcodeAgg
{
    public class ListBrgBarcodeChangedQuery : IRequest<IEnumerable<BrgBarcodeModel>>
    {
        public ListBrgBarcodeChangedQuery(byte[] watermark) => Watermark = watermark;
        public byte[] Watermark { get; }
    }

    public class ListBrgBarcodeChangedHandler
        : IRequestHandler<ListBrgBarcodeChangedQuery, IEnumerable<BrgBarcodeModel>>
    {
        private readonly IBrgBarcodeDal _brgBarcodeDal;

        public ListBrgBarcodeChangedHandler(IBrgBarcodeDal brgBarcodeDal)
        {
            _brgBarcodeDal = brgBarcodeDal;
        }

        public Task<IEnumerable<BrgBarcodeModel>> Handle(ListBrgBarcodeChangedQuery request,
            CancellationToken cancellationToken)
        {
            //  GUARD
            Guard.Argument(() => request).NotNull();

            //  QUERY — IR-04 change feed: rows with RowVer > watermark, ordered by RowVer
            var result = _brgBarcodeDal.ListChanged(request.Watermark)
                         ?? Enumerable.Empty<BrgBarcodeModel>();

            //  RESPONSE
            return Task.FromResult(result);
        }
    }
}
