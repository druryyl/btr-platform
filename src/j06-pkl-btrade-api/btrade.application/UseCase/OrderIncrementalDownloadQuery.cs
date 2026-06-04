using btrade.application.Contract;
using btrade.domain.SalesFeature;
using MediatR;
using Nuna.Lib.TransactionHelper;
using Nuna.Lib.ValidationHelper;

namespace btrade.application.UseCase
{
    public record OrderIncrementalDownloadQuery(string Tgl1, string Tgl2, string ServerId)
        : IRequest<IEnumerable<OrderModel>>, IServerId;

    public class OrderIncrementalDownloadQueryHandler : IRequestHandler<OrderIncrementalDownloadQuery, IEnumerable<OrderModel>>
    {
        private readonly IOrderDal _orderDal;
        private readonly IOrderItemDal _orderItemDal;
        public OrderIncrementalDownloadQueryHandler(IOrderDal orderDal, IOrderItemDal orderItemDal)
        {
            _orderDal = orderDal;
            _orderItemDal = orderItemDal;
        }
        public Task<IEnumerable<OrderModel>> Handle(OrderIncrementalDownloadQuery request, CancellationToken cancellationToken)
        {
            var periode = new Periode(request.Tgl1.ToDate(DateFormatEnum.YMD), 
                request.Tgl2.ToDate(DateFormatEnum.YMD));
            var orders = _orderDal.ListData(periode, request);
            if (orders.HasValue == false)
                return Task.FromResult(Enumerable.Empty<OrderModel>());

            var orderSent = orders.Value.Where(x => x.StatusSync == "TERKIRIM").ToList();
            if (orderSent.Count == 0)
                return Task.FromResult(Enumerable.Empty<OrderModel>());

            var result = new List<OrderModel>();
            foreach (var order in orderSent)
            {
                var listItem = _orderItemDal.ListData(order)?.ToList() ?? new List<OrderItemType>();
                if (listItem.Count == 0)
                    continue;
                order.ListItems = listItem;
                result.Add(order);
            }

            using var trans = TransHelper.NewScope();
            foreach (var order in result)
            {
                order.StatusSync = "DOWNLOADED";
                _orderDal.Update(order);
            }
            trans.Complete();

            return Task.FromResult(result.AsEnumerable());
        }
    }
}
