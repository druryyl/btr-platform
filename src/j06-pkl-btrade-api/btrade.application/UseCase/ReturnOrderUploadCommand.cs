using btrade.application.Contract;
using btrade.domain.ReturnOrderFeature;
using btrade.domain.SalesFeature;
using MediatR;
using Nuna.Lib.TransactionHelper;

namespace btrade.application.UseCase
{
    public record ReturnOrderUploadCommand(
        string ReturnOrderId, string ReturnOrderDate, string WarehouseCode,
        string CustomerId, string CustomerName,
        string SalesPersonId, string SalesPersonName,
        string DriverId, string DriverName,
        string Note, string ServerId, string SubmittedBy,
        IEnumerable<ReturnOrderItemType> ListItem) : IRequest, IServerId;

    public class ReturnOrderUploadCommandHandler : IRequestHandler<ReturnOrderUploadCommand>
    {
        private readonly IReturnOrderDal _returnOrderDal;
        private readonly IReturnOrderItemDal _returnOrderItemDal;
        public ReturnOrderUploadCommandHandler(IReturnOrderDal returnOrderDal, IReturnOrderItemDal returnOrderItemDal)
        {
            _returnOrderDal = returnOrderDal;
            _returnOrderItemDal = returnOrderItemDal;
        }
        public Task Handle(ReturnOrderUploadCommand request, CancellationToken cancellationToken)
        {
            var model = new ReturnOrderType(
                request.ReturnOrderId, request.ServerId, request.ReturnOrderDate,
                request.WarehouseCode, request.CustomerId, request.CustomerName,
                request.SalesPersonId, request.SalesPersonName,
                request.DriverId, request.DriverName,
                request.Note, "TERKIRIM", request.SubmittedBy);

            foreach (var item in request.ListItem)
            {
                model.ListItems.Add(item);
            }
            using var trans = TransHelper.NewScope();

            _returnOrderDal.Delete(ReturnOrderType.Key(request.ReturnOrderId));
            _returnOrderDal.Insert(model);
            _returnOrderItemDal.Delete(ReturnOrderType.Key(request.ReturnOrderId));
            _returnOrderItemDal.Insert(model.ListItems);

            trans.Complete();
            return Task.CompletedTask;
        }
    }
}
