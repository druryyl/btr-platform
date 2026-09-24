using btrade.application.Contract;
using btrade.application.UseCase;
using btrade.domain.WarehouseFeature;
using Nuna.Lib.PatternHelper;

namespace btrade.webapi.Test;

/// <summary>
/// In-memory IWarehouseMappingDal seeded exactly like
/// btrade.sqldb/Scripts/Create_BTR_WarehouseMapping.sql
/// (GAMPING/CONCAT/MAGELANG as stored — no invented vocabulary).
/// </summary>
internal class FakeWarehouseMappingDal : IWarehouseMappingDal
{
    private static readonly WarehouseMappingType[] Seed =
    {
        new("GAMPING", "JOGJA"),
        new("CONCAT", "JOGJA"),
        new("MAGELANG", "MGL")
    };

    public MayBe<WarehouseMappingType> GetData(IWarehouseMappingKey key) =>
        Seed.FirstOrDefault(mapping => mapping.WarehouseCode == key.WarehouseCode) is { } found
            ? MayBe.From(found)
            : MayBe<WarehouseMappingType>.None;

    public MayBe<IEnumerable<WarehouseMappingType>> ListData() =>
        MayBe.From(Seed.AsEnumerable());
}
