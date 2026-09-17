namespace btrade.domain.BarcodeFeature;

public record UserType(
    string UserId,
    string UserName,
    string Password,
    string RoleId,
    bool IsAktif,
    string ServerId) : IUserKey;

public interface IUserKey
{
    string UserId { get; }
}
