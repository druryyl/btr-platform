using btrade.application.Contract;
using btrade.domain.DriverFeature;
using btrade.domain.SalesFeature;
using Nuna.Lib.PatternHelper;

namespace btrade.webapi.Test;

/// <summary>
/// In-memory IDriverDal (the FakeBarcodeDal convention) so the anonymous
/// Driver routes can be exercised without a database.
/// </summary>
internal class FakeDriverDal : IDriverDal
{
    private readonly List<DriverType> _items = new();

    public IReadOnlyList<DriverType> Items => _items;

    public void Insert(DriverType model) => _items.Add(model);

    public void Update(DriverType model)
    {
        var index = _items.FindIndex(
            x => x.DriverId == model.DriverId && x.ServerId == model.ServerId);
        if (index >= 0)
            _items[index] = model;
    }

    public void Delete(IDriverKey key) =>
        _items.RemoveAll(x => x.DriverId == key.DriverId && x.ServerId == key.ServerId);

    public void Delete(IServerId server) =>
        _items.RemoveAll(x => x.ServerId == server.ServerId);

    public MayBe<DriverType> GetData(IDriverKey key)
    {
        var found = _items.FirstOrDefault(
            x => x.DriverId == key.DriverId && x.ServerId == key.ServerId);
        return found is null ? MayBe<DriverType>.None : MayBe.From(found);
    }

    public MayBe<IEnumerable<DriverType>> ListData(IServerId server) =>
        MayBe.From(_items.Where(x => x.ServerId == server.ServerId).AsEnumerable());
}
