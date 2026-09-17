using btrade.application.Contract;
using btrade.domain.BarcodeFeature;
using btrade.domain.SalesFeature;
using Nuna.Lib.PatternHelper;

namespace btrade.webapi.Test;

internal class FakeBarcodeDal : IBarcodeDal
{
    public void Insert(BarcodeType model) { }

    public void Update(BarcodeType model) { }

    public void Delete(IBarcodeKey key) { }

    public MayBe<BarcodeType> GetData(IBarcodeKey key) => MayBe<BarcodeType>.None;

    public MayBe<IEnumerable<BarcodeType>> ListData(IServerId server) =>
        MayBe.From(Enumerable.Empty<BarcodeType>());
}
