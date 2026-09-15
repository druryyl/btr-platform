using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using btr.domain.BrgContext.BrgBarcodeAgg;
using btr.nuna.Domain;
using MediatR;

namespace btr.application.BrgContext.BrgBarcodeAgg
{
    public class ListBrgBarcodeQuery : IRequest<IEnumerable<BrgBarcodeModel>>
    {
        public ListBrgBarcodeQuery(string keyword = "") => Keyword = keyword ?? string.Empty;
        public string Keyword { get; }
    }

    public class ListBrgBarcodeHandler : IRequestHandler<ListBrgBarcodeQuery, IEnumerable<BrgBarcodeModel>>
    {
        private readonly IBrgBarcodeDal _brgBarcodeDal;

        public ListBrgBarcodeHandler(IBrgBarcodeDal brgBarcodeDal)
        {
            _brgBarcodeDal = brgBarcodeDal;
        }

        public Task<IEnumerable<BrgBarcodeModel>> Handle(ListBrgBarcodeQuery request,
            CancellationToken cancellationToken)
        {
            //  QUERY — search/filter across barcode value, Item code, Item name
            var listBarcode = _brgBarcodeDal.ListData();
            if (listBarcode.IsNullOrEmpty())
                return Task.FromResult(Enumerable.Empty<BrgBarcodeModel>());

            //  RESPONSE
            if (string.IsNullOrWhiteSpace(request.Keyword))
                return Task.FromResult(listBarcode);

            var keywords = request.Keyword.ToLower().Split(' ');
            var result = listBarcode.Where(x => keywords.All(word =>
                (x.BarcodeValue ?? string.Empty).ToLower().Contains(word) ||
                (x.BrgCode ?? string.Empty).ToLower().Contains(word) ||
                (x.BrgName ?? string.Empty).ToLower().Contains(word)));
            return Task.FromResult(result);
        }
    }
}
