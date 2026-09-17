namespace btrade.domain.BarcodeFeature;

public record LocationType(
    string LocationId,
    string LocationName,
    string ServerId) : ILocationKey;

public interface ILocationKey
{
    string LocationId { get; }
}
