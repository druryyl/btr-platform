using btrade.application.Contract;
using btrade.domain.ReturnOrderFeature;
using btrade.domain.SalesFeature;
using Nuna.Lib.PatternHelper;
using Nuna.Lib.ValidationHelper;

namespace btrade.webapi.Test;

/// <summary>
/// In-memory IReturnOrderDal (the FakeBarcodeDal convention). Its ListData
/// mirrors the real DAL's ServerId-scoped read so the incremental download's
/// SubmittedBy output can be exercised without a database; the date range is
/// not applied (tests seed rows already inside the queried window).
/// </summary>
internal class FakeReturnOrderDal : IReturnOrderDal
{
    private readonly List<ReturnOrderType> _items = new();

    public IReadOnlyList<ReturnOrderType> Items => _items;

    public void Insert(ReturnOrderType model) => _items.Add(model);

    public void Update(ReturnOrderType model)
    {
        var index = _items.FindIndex(
            x => x.ReturnOrderId == model.ReturnOrderId && x.ServerId == model.ServerId);
        if (index >= 0)
            _items[index] = model;
    }

    public void Delete(IReturnOrderKey key) =>
        _items.RemoveAll(x => x.ReturnOrderId == key.ReturnOrderId);

    public void Delete(IServerId server) =>
        _items.RemoveAll(x => x.ServerId == server.ServerId);

    public MayBe<ReturnOrderType> GetData(IReturnOrderKey key)
    {
        var found = _items.FirstOrDefault(x => x.ReturnOrderId == key.ReturnOrderId);
        return found is null ? MayBe<ReturnOrderType>.None : MayBe.From(found);
    }

    public MayBe<IEnumerable<ReturnOrderType>> ListData(Periode periode, IServerId server) =>
        MayBe.From(_items.Where(x => x.ServerId == server.ServerId).AsEnumerable());
}
