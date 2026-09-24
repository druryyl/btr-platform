using btrade.application.Contract;
using btrade.domain.ReturnOrderFeature;

namespace btrade.webapi.Test;

/// <summary>In-memory IReturnOrderItemDal (the FakeBarcodeDal convention).</summary>
internal class FakeReturnOrderItemDal : IReturnOrderItemDal
{
    private readonly List<ReturnOrderItemType> _items = new();

    public void Insert(IEnumerable<ReturnOrderItemType> listModel) =>
        _items.AddRange(listModel);

    public void Delete(IReturnOrderKey key) =>
        _items.RemoveAll(x => x.ReturnOrderId == key.ReturnOrderId);

    public IEnumerable<ReturnOrderItemType> ListData(IReturnOrderKey filter) =>
        _items.Where(x => x.ReturnOrderId == filter.ReturnOrderId);
}
