using btrade.application.Contract;
using btrade.domain.BarcodeFeature;
using btrade.domain.WarehouseFeature;

namespace btrade.application.UseCase;

/// <summary>
/// TD-04 — the single owner of Cloud session-context resolution.
/// Both the anonymous resolution endpoint (TD-02, <c>POST api/session/resolve</c)
/// and the four BGud-consumed routes (TD-03) resolve tenant and actor through
/// this component; no controller reads JWT claims on BGud's path. It reuses
/// the existing <see cref="IUserDal"/> (<c>BTRADE_User.Email</c>) and
/// <see cref="IWarehouseMappingDal"/> (<c>BTR_WarehouseMapping</c>) — no new
/// tenant table or vocabulary is introduced.
///
/// Failure signalling is deliberately typed so later slices can map the two
/// conditions to their distinct HTTP outcomes per §9/TD-13:
/// <see cref="SessionAccountUnresolvableException"/> (actor unresolvable) and
/// <see cref="SessionTenantUnresolvableException"/> (location unmapped).
/// </summary>
public class SessionContextResolver
{
    private const string AccountUnresolvedMessage =
        "Google account is not registered or is not active. Contact your administrator.";

    private readonly IUserDal _userDal;
    private readonly IWarehouseMappingDal _warehouseMappingDal;

    public SessionContextResolver(
        IUserDal userDal,
        IWarehouseMappingDal warehouseMappingDal)
    {
        _userDal = userDal;
        _warehouseMappingDal = warehouseMappingDal;
    }

    /// <summary>
    /// TD-02/TD-04 — resolve a Google email to the authoritative BTR user
    /// (UserId/UserName/RoleId). Rejects an unmapped or invalid account
    /// (not found, blank, or IsAktif false) explicitly, mirroring
    /// <c>IssueTokenCommand</c>'s fail-explicitly behaviour. Emails are
    /// trimmed and compared case-insensitively (TD-14).
    /// </summary>
    public SessionAccount ResolveAccount(string email)
    {
        var normalizedEmail = (email ?? string.Empty).Trim();
        if (normalizedEmail.Length == 0)
            throw new SessionAccountUnresolvableException(AccountUnresolvedMessage);

        var user = _userDal.GetByEmail(normalizedEmail);
        if (!user.HasValue || !user.Value.IsAktif)
            throw new SessionAccountUnresolvableException(AccountUnresolvedMessage);

        return new SessionAccount(
            user.Value.UserId,
            user.Value.UserName,
            user.Value.RoleId);
    }

    /// <summary>
    /// TD-04 — resolve a session locationId to its tenant ServerId through
    /// BTR_WarehouseMapping. An unmapped code fails explicitly with no
    /// fallback, consistent with the existing <c>IssueTokenCommand</c>
    /// mapping behaviour (the message mirrors it verbatim).
    /// </summary>
    public string ResolveTenant(string locationId)
    {
        var mapping = _warehouseMappingDal.GetData(
            new WarehouseMappingKey(locationId ?? string.Empty));
        if (!mapping.HasValue)
            throw new SessionTenantUnresolvableException(
                $"Invalid warehouse ({locationId})");

        return mapping.Value.ServerId;
    }

    /// <summary>
    /// TD-02/TD-04 — the full distinct locationId(WarehouseCode)→ServerId
    /// mapping returned to BGud at session establishment for its legacy
    /// {serverId} routes.
    /// </summary>
    public IEnumerable<WarehouseMappingType> ListWarehouses()
    {
        return _warehouseMappingDal
            .ListData()
            .GetValueOrDefault(Enumerable.Empty<WarehouseMappingType>());
    }
}

/// <summary>The authoritative BTR identity resolved for a Google email (TD-02).</summary>
public record SessionAccount(
    string UserId,
    string UserName,
    string RoleId);

/// <summary>
/// The Google email is unmapped or the account is invalid (not found or
/// inactive). At <c>POST api/session/resolve</c> this is surfaced as
/// HTTP 400 (§9); TD-13 lets later slices map the same condition to
/// HTTP 409 on the header-carrying routes. Deliberately not an
/// <see cref="ArgumentException"/> so the two outcomes stay distinguishable.
/// </summary>
public class SessionAccountUnresolvableException : Exception
{
    public SessionAccountUnresolvableException(string message) : base(message)
    {
    }
}

/// <summary>
/// The session locationId has no BTR_WarehouseMapping row. Derives from
/// <see cref="ArgumentException"/> so the existing ErrorHandlerMiddleware
/// mapping yields HTTP 400 — exactly what <c>IssueTokenCommand</c>'s
/// unmapped-warehouse failure produces today.
/// </summary>
public class SessionTenantUnresolvableException : ArgumentException
{
    public SessionTenantUnresolvableException(string message) : base(message)
    {
    }
}
