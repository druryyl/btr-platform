using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using btr.domain.BrgContext.BrgAgg;
using btr.domain.BrgContext.BrgBarcodeAgg;
using Dawn;
using MediatR;

namespace btr.application.BrgContext.BrgBarcodeAgg
{
    public class ListBrgBarcodeByBrgQuery : IRequest<IEnumerable<BrgBarcodeModel>>, IBrgKey
    {
        public ListBrgBarcodeByBrgQuery(string brgId) => BrgId = brgId;
        public string BrgId { get; }
    }

    public class ListBrgBarcodeByBrgHandler
        : IRequestHandler<ListBrgBarcodeByBrgQuery, IEnumerable<BrgBarcodeModel>>
    {
        private readonly IBrgBarcodeDal _brgBarcodeDal;

        public ListBrgBarcodeByBrgHandler(IBrgBarcodeDal brgBarcodeDal)
        {
            _brgBarcodeDal = brgBarcodeDal;
        }

        public Task<IEnumerable<BrgBarcodeModel>> Handle(ListBrgBarcodeByBrgQuery request,
            CancellationToken cancellationToken)
        {
            //  GUARD
            Guard.Argument(() => request).NotNull()
                .Member(x => x.BrgId, y => y.NotEmpty());

            //  QUERY — UC-004: all lifecycle states for one Item
            var result = _brgBarcodeDal.ListByBrg(new BrgModel(request.BrgId))
                         ?? Enumerable.Empty<BrgBarcodeModel>();

            //  RESPONSE
            return Task.FromResult(result);
        }
    }
}
