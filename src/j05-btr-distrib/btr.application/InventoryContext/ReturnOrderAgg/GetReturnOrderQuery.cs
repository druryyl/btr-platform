using System.Threading;
using System.Threading.Tasks;
using btr.application.InventoryContext.ReturnOrderAgg.Workers;
using btr.domain.InventoryContext.ReturnOrderAgg;
using Dawn;
using MediatR;

namespace btr.application.InventoryContext.ReturnOrderAgg
{
    public class GetReturnOrderQuery : IRequest<ReturnOrderModel>, IReturnOrderKey
    {
        public GetReturnOrderQuery(string returnOrderId) => ReturnOrderId = returnOrderId;
        public string ReturnOrderId { get; }
    }

    public class GetReturnOrderHandler : IRequestHandler<GetReturnOrderQuery, ReturnOrderModel>
    {
        private ReturnOrderModel _aggRoot = new ReturnOrderModel();
        private readonly IReturnOrderBuilder _builder;

        public GetReturnOrderHandler(IReturnOrderBuilder builder)
        {
            _builder = builder;
        }

        public Task<ReturnOrderModel> Handle(GetReturnOrderQuery request,
            CancellationToken cancellationToken)
        {
            //  GUARD
            Guard.Argument(() => request).NotNull()
                .Member(x => x.ReturnOrderId, y => y.NotEmpty());

            //  BUILD — header + item lines, with the read-convenience names
            //  resolved on load (§7.1)
            _aggRoot = _builder
                .Load(request)
                .Build();

            //  RESPONSE
            return Task.FromResult(_aggRoot);
        }
    }
}
