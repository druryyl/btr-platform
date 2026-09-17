using btrade.domain.SalesFeature;

namespace btrade.domain.DriverFeature;

public record DriverType(string DriverId, string DriverName, bool IsAktif, string ServerId)
    : IDriverKey, IServerId;

public interface IDriverKey : IServerId
{
    string DriverId { get; }
}
