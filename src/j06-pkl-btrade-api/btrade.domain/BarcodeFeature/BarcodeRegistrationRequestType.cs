namespace btrade.domain.BarcodeFeature;

public record BarcodeRegistrationRequestType(
    string BarcodeRegistrationId,
    string ClientRequestId,
    string ServerId,
    string BarcodeValue,
    string BrgId,
    string Satuan,
    string RequestedBy,
    DateTime RequestedAt,
    string Status,
    DateTime? ProcessedAt,
    string ProcessedNote) : IBarcodeRegistrationRequestKey;

public interface IBarcodeRegistrationRequestKey
{
    string BarcodeRegistrationId { get; }
}
