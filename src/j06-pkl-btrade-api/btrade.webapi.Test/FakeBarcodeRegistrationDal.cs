using btrade.application.Contract;
using btrade.domain.BarcodeFeature;
using btrade.domain.SalesFeature;
using Nuna.Lib.PatternHelper;

namespace btrade.webapi.Test;

/// <summary>
/// In-memory IBarcodeRegistrationDal (the FakeBarcodeDal convention). Its
/// ListData mirrors the real DAL's ServerId-scoped read so the status query's
/// RequestedBy filter can be exercised without a database.
/// </summary>
internal class FakeBarcodeRegistrationDal : IBarcodeRegistrationDal
{
    private readonly List<BarcodeRegistrationRequestType> _items = new();

    public IReadOnlyList<BarcodeRegistrationRequestType> Items => _items;

    public void Insert(BarcodeRegistrationRequestType model) => _items.Add(model);

    public void Update(BarcodeRegistrationRequestType model)
    {
        var index = _items.FindIndex(
            x => x.BarcodeRegistrationId == model.BarcodeRegistrationId);
        if (index >= 0)
            _items[index] = model;
    }

    public void Delete(IBarcodeRegistrationRequestKey key) =>
        _items.RemoveAll(x => x.BarcodeRegistrationId == key.BarcodeRegistrationId);

    public MayBe<BarcodeRegistrationRequestType> GetData(IBarcodeRegistrationRequestKey key)
    {
        var found = _items.FirstOrDefault(
            x => x.BarcodeRegistrationId == key.BarcodeRegistrationId);
        return found is null
            ? MayBe<BarcodeRegistrationRequestType>.None
            : MayBe.From(found);
    }

    public MayBe<IEnumerable<BarcodeRegistrationRequestType>> ListData(IServerId server) =>
        MayBe.From(_items.Where(x => x.ServerId == server.ServerId).AsEnumerable());
}
