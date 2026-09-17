using btr.application.InventoryContext.ReturnOrderAgg.Contracts;
using btr.domain.InventoryContext.ReturnOrderAgg;
using btr.nuna.Application;

namespace btr.application.InventoryContext.ReturnOrderAgg.Workers
{
    public interface IReturnOrderWriter : INunaWriter2<ReturnOrderModel>
    {
    }

    public class ReturnOrderWriter : IReturnOrderWriter
    {
        private readonly IReturnOrderDal _returnOrderDal;
        private readonly IReturnOrderItemDal _returnOrderItemDal;

        public ReturnOrderWriter(IReturnOrderDal returnOrderDal,
            IReturnOrderItemDal returnOrderItemDal)
        {
            _returnOrderDal = returnOrderDal;
            _returnOrderItemDal = returnOrderItemDal;
        }

        public ReturnOrderModel Save(ReturnOrderModel model)
        {
            var returnOrderId = model.ReturnOrderId;
            model.ListItem.ForEach(x => x.ReturnOrderId = returnOrderId);

            var db = _returnOrderDal.GetData(model);

            using (var trans = TransHelper.NewScope())
            {
                if (db is null)
                    _returnOrderDal.Insert(model);
                else
                    _returnOrderDal.Update(model);

                _returnOrderItemDal.Delete(model);
                _returnOrderItemDal.Insert(model.ListItem);
                trans.Complete();
            }
            return model;
        }
    }
}
