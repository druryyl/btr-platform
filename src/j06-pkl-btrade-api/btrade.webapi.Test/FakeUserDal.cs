using btrade.application.Contract;
using btrade.domain.BarcodeFeature;
using btrade.domain.SalesFeature;
using Nuna.Lib.PatternHelper;

namespace btrade.webapi.Test;

/// <summary>
/// In-memory IUserDal for session-resolve tests (the FakeBarcodeDal
/// convention): stores by UserId, and mirrors UserDal.GetByEmail's TD-14
/// semantics (trim + case-insensitive + empty-email never matches).
/// </summary>
internal class FakeUserDal : IUserDal
{
    private readonly Dictionary<string, UserType> _users = new();

    public void Insert(UserType model)
    {
        //  Mirrors the real DAL's NOT NULL DEFAULT('') guard (EXT-02).
        _users[model.UserId] = model with { Email = model.Email ?? string.Empty };
    }

    public void Delete(IServerId server)
    {
        var stale = _users.Values
            .Where(user => user.ServerId == server.ServerId)
            .Select(user => user.UserId)
            .ToList();
        foreach (var userId in stale)
            _users.Remove(userId);
    }

    public MayBe<UserType> GetData(IUserKey key) =>
        _users.TryGetValue(key.UserId, out var user)
            ? MayBe.From(user)
            : MayBe<UserType>.None;

    public MayBe<UserType> GetByEmail(string email)
    {
        var normalized = (email ?? string.Empty).Trim();
        if (normalized.Length == 0)
            return MayBe<UserType>.None;

        var user = _users.Values.FirstOrDefault(candidate =>
            !string.IsNullOrEmpty(candidate.Email) &&
            string.Equals(candidate.Email.Trim(), normalized,
                StringComparison.OrdinalIgnoreCase));
        return user is null ? MayBe<UserType>.None : MayBe.From(user);
    }
}
