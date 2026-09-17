namespace btrade.domain.WarehouseFeature;

public record WarehouseMappingType(
    string WarehouseCode,
    string ServerId) : IWarehouseMappingKey;

public interface IWarehouseMappingKey
{
    string WarehouseCode { get; }
}
