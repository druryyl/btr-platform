namespace btrade.webapi.Infrastructure;

/// <summary>
/// §10 — the fixed session-context header names (TD-03). The values are plain
/// operational context (locationId / Google email), not credentials; they are
/// read only to feed <c>SessionContextResolver</c> and are never logged at
/// informational level (§9).
/// </summary>
public static class SessionContextHeaders
{
    public const string Location = "X-Session-Location";
    public const string Actor = "X-Session-Actor";

    /// <summary>The carried locationId, or an empty string when absent.</summary>
    public static string GetSessionLocation(this HttpRequest request)
        => request.Headers[Location].ToString();

    /// <summary>The carried Google email, or an empty string when absent.</summary>
    public static string GetSessionActor(this HttpRequest request)
        => request.Headers[Actor].ToString();
}
