using btrade.application.Contract;
using btrade.domain.ReturnOrderFeature;
using btrade.domain.SalesFeature;
using MediatR;
using Nuna.Lib.TransactionHelper;
using Nuna.Lib.ValidationHelper;

namespace btrade.application.UseCase
{
    public record ReturnOrderIncrementalDownloadQuery(string Tgl1, string Tgl2, string ServerId)
        : IRequest<IEnumerable<ReturnOrderType>>, IServerId;

    public class ReturnOrderIncrementalDownloadQueryHandler
        : IRequestHandler<ReturnOrderIncrementalDownloadQuery, IEnumerable<ReturnOrderType>>
    {
        private readonly IReturnOrderDal _returnOrderDal;
        private readonly IReturnOrderItemDal _returnOrderItemDal;
        public ReturnOrderIncrementalDownloadQueryHandler(IReturnOrderDal returnOrderDal, IReturnOrderItemDal returnOrderItemDal)
        {
            _returnOrderDal = returnOrderDal;
            _returnOrderItemDal = returnOrderItemDal;
        }
        public Task<IEnumerable<ReturnOrderType>> Handle(ReturnOrderIncrementalDownloadQuery request, CancellationToken cancellationToken)
        {
            var periode = new Periode(request.Tgl1.ToDate(DateFormatEnum.YMD),
                request.Tgl2.ToDate(DateFormatEnum.YMD));
            var orders = _returnOrderDal.ListData(periode, request);
            if (orders.HasValue == false)
                return Task.FromResult(Enumerable.Empty<ReturnOrderType>());

            var orderSent = orders.Value.Where(x => x.StatusSync == "TERKIRIM").ToList();
            if (orderSent.Count == 0)
                return Task.FromResult(Enumerable.Empty<ReturnOrderType>());

            var result = new List<ReturnOrderType>();
            foreach (var order in orderSent)
            {
                var listItem = _returnOrderItemDal.ListData(order)?.ToList() ?? new List<ReturnOrderItemType>();
                order.ListItems = listItem;
                result.Add(order);
            }

            using var trans = TransHelper.NewScope();
            foreach (var order in result)
            {
                order.StatusSync = "DOWNLOADED";
                _returnOrderDal.Update(order);
            }
            trans.Complete();

            return Task.FromResult(result.AsEnumerable());
        }
    }
}
