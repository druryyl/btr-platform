using btrade.domain.SalesFeature;

namespace btrade.domain.BarcodeFeature;

public record BarcodeType(
    string BrgBarcodeId,
    string BarcodeValue,
    string BrgId,
    string BrgCode,
    string BrgName,
    string Satuan,
    string ServerId) : IBarcodeKey;

public interface IBarcodeKey : IServerId
{
    string BrgBarcodeId { get; }
}
