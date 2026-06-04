using btrade.application.Contract;
using btrade.domain.SalesFeature;
using MediatR;
using Nuna.Lib.TransactionHelper;

namespace btrade.application.UseCase
{
    public record OrderUploadCommand(string OrderId, string OrderLocalId, 
        string CustomerId, string CustomerCode, string CustomerName, string Address,
        string OrderDate, string SalesId, string SalesName, decimal TotalAmount, 
        string UserEmail, string OrderNote, string ServerId, IEnumerable<OrderItemType> ListItem) : IRequest, IServerId;

    public class OrderUploadCommandHandler : IRequestHandler<OrderUploadCommand>
    {
        private readonly IOrderDal _orderDal;
        private readonly IOrderItemDal _orderItemDal;
        public OrderUploadCommandHandler(IOrderDal orderDal, IOrderItemDal orderItemDal)
        {
            _orderDal = orderDal;
            _orderItemDal = orderItemDal;
        }
        public Task Handle(OrderUploadCommand request, CancellationToken cancellationToken)
        {
            var model = new OrderModel(
                request.OrderId, request.OrderLocalId, request.CustomerId,
                request.CustomerCode, request.CustomerName, request.Address,
                request.OrderDate, request.SalesId, request.SalesName, 
                request.TotalAmount, request.UserEmail, "TERKIRIM", "", 
                request.OrderNote, request.ServerId);

            foreach(var item in request.ListItem)
            {
                model.ListItems.Add(item);
            }
            using var trans = TransHelper.NewScope();
            
            _orderDal.Delete(OrderModel.Key(request.OrderId));
            _orderDal.Insert(model);
            _orderItemDal.Delete(OrderModel.Key(request.OrderId));
            _orderItemDal.Insert(model.ListItems);

            trans.Complete();
            return Task.CompletedTask;
        }
    }
}
