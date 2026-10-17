using System;
using System.Text.Json;
using System.Threading.Tasks;
using BtrReturGudang.AppTier.Contracts;
using BtrReturGudang.AppTier.DTOs;

namespace BtrReturGudang.Winform.Services
{
    /// <summary>
    /// Presentation-layer operator session validation gate for the workstation.
    /// Resolves the configured operator Google email through the Cloud anonymous
    /// endpoint <c>POST api/session/resolve</c>
    /// (<see cref="IBtradeApiClient.ResolveSessionAsync(string)"/>) and caches the
    /// last outcome so upload actions can be pre-empted with the Cloud refusal
    /// message instead of an opaque HTTP 409.
    /// Traceable to: ARCHITECTURE §4 TD-012, §5.2 SCR-07, P2-S05.
    /// </summary>
    public class OperatorSessionValidator
    {
        /// <summary>
        /// Fallback refusal text shown when the Cloud returns no usable message.
        /// </summary>
        public const string DefaultRefusalMessage =
            "Google account is not registered or is not active. Contact your administrator.";

        private readonly IBtradeApiClient _apiClient;
        private readonly IRegistryHelper _registryHelper;
        private readonly object _syncRoot = new object();
        private SessionResolveResultDto _lastResult;

        public OperatorSessionValidator(IBtradeApiClient apiClient, IRegistryHelper registryHelper)
        {
            _apiClient = apiClient;
            _registryHelper = registryHelper;
        }

        /// <summary>
        /// The currently configured operator Google email (Cloud session actor).
        /// </summary>
        public string OperatorEmail => _registryHelper?.LastOperatorEmail ?? string.Empty;

        /// <summary>
        /// The most recent resolution outcome, or <c>null</c> when validation has not run.
        /// </summary>
        public SessionResolveResultDto LastResult
        {
            get { lock (_syncRoot) { return _lastResult; } }
        }

        /// <summary>
        /// True when a resolution has been attempted (successful or not).
        /// </summary>
        public bool HasBeenValidated
        {
            get { lock (_syncRoot) { return _lastResult != null; } }
        }

        /// <summary>
        /// True when the last resolution succeeded and the operator identity is authoritative.
        /// </summary>
        public bool IsResolved
        {
            get { lock (_syncRoot) { return _lastResult != null && _lastResult.Success; } }
        }

        /// <summary>
        /// True when the configured operator email is definitively unresolvable and upload
        /// operations must be pre-empted with <see cref="RefusalMessage"/>.
        /// A not-yet-performed validation or a transport failure (no server verdict) does
        /// not block uploads, preserving the P1-S03 offline-tolerant upload path.
        /// </summary>
        public bool IsUnresolved
        {
            get
            {
                lock (_syncRoot)
                {
                    if (_lastResult == null) return false;
                    if (_lastResult.Success) return false;

                    // A blank configured email can never resolve; it is a definite refusal.
                    if (string.IsNullOrWhiteSpace(OperatorEmail)) return true;

                    // Only the Cloud's definite sign-in refusal verdict (e.g. 400/403/409)
                    // blocks uploads. A transient 5xx has no refusal verdict and is treated
                    // like a transport failure, so it does not falsely block the
                    // offline-tolerant upload path (ARCHITECTURE §4 TD-012, §11.13).
                    return IsDefiniteRefusal(_lastResult.StatusCode);
                }
            }
        }

        /// <summary>
        /// True when the status code represents the Cloud's definite sign-in refusal
        /// (an unmapped or inactive operator account) rather than a transient server or
        /// transport failure. Traceable to: ARCHITECTURE §4 TD-012 item 2, §11.13.
        /// </summary>
        private static bool IsDefiniteRefusal(int statusCode)
        {
            return statusCode == 400 || statusCode == 403 || statusCode == 409;
        }

        /// <summary>
        /// The Cloud refusal message to surface to the operator. The Cloud emits its
        /// refusal as a JSend failure envelope (<c>{ "status": ..., "code": ...,
        /// "data": "&lt;message&gt;" }</c>, see j06 <c>SessionController.Resolve</c>), so
        /// the human-readable message is unwrapped from the carried response. The raw
        /// JSON envelope is never surfaced; when no usable message exists the
        /// <see cref="DefaultRefusalMessage"/> is returned.
        /// </summary>
        public string RefusalMessage
        {
            get
            {
                lock (_syncRoot)
                {
                    return ExtractRefusalMessage(_lastResult);
                }
            }
        }

        /// <summary>
        /// Resolves the operator-facing refusal text for a failed session resolution.
        /// The Cloud refusal is a JSend failure envelope whose <c>data</c> (fallback
        /// <c>message</c>) holds the human-readable message; that message is extracted
        /// from the carried raw response or error detail so the operator sees the Cloud
        /// refusal and never the raw JSON envelope. A non-JSON server detail is surfaced
        /// as-is, and when no usable message exists the
        /// <see cref="DefaultRefusalMessage"/> is returned.
        /// Traceable to: ARCHITECTURE §4 TD-012 item 2/3, §11.13, P2-S05 (RV-001).
        /// </summary>
        public static string ExtractRefusalMessage(SessionResolveResultDto result)
        {
            if (result == null) return DefaultRefusalMessage;

            foreach (var candidate in new[] { result.RawResponse, result.ErrorMessage })
            {
                if (string.IsNullOrWhiteSpace(candidate)) continue;

                var parsed = ExtractJsEndMessage(candidate);
                if (!string.IsNullOrWhiteSpace(parsed)) return parsed.Trim();

                // A JSON body with no usable message must never be surfaced verbatim.
                if (LooksLikeJsonObject(candidate)) continue;

                // Non-JSON detail (e.g. a transport message) is already human-readable.
                return candidate.Trim();
            }

            return DefaultRefusalMessage;
        }

        private static bool LooksLikeJsonObject(string body)
        {
            var trimmed = body.TrimStart();
            return trimmed.Length > 0 && trimmed[0] == '{';
        }

        private static string ExtractJsEndMessage(string body)
        {
            if (string.IsNullOrWhiteSpace(body)) return null;
            if (!LooksLikeJsonObject(body)) return null;

            try
            {
                using (var doc = JsonDocument.Parse(body))
                {
                    var root = doc.RootElement;
                    if (root.ValueKind != JsonValueKind.Object) return null;

                    // JSend failure: the human-readable message sits in "data".
                    if (TryGetPropertyIgnoreCase(root, "data", out var data))
                    {
                        var fromData = ReadMessage(data);
                        if (!string.IsNullOrWhiteSpace(fromData)) return fromData;
                    }

                    // JSend also permits a top-level "message".
                    if (TryGetPropertyIgnoreCase(root, "message", out var message)
                        && message.ValueKind == JsonValueKind.String)
                    {
                        var text = message.GetString();
                        if (!string.IsNullOrWhiteSpace(text)) return text;
                    }

                    return null;
                }
            }
            catch
            {
                return null;
            }
        }

        private static string ReadMessage(JsonElement data)
        {
            switch (data.ValueKind)
            {
                case JsonValueKind.String:
                    return data.GetString();

                case JsonValueKind.Object:
                    if (TryGetPropertyIgnoreCase(data, "message", out var nested)
                        && nested.ValueKind == JsonValueKind.String)
                    {
                        return nested.GetString();
                    }

                    if (TryGetPropertyIgnoreCase(data, "data", out var nestedData)
                        && nestedData.ValueKind == JsonValueKind.String)
                    {
                        return nestedData.GetString();
                    }

                    return null;

                default:
                    return null;
            }
        }

        private static bool TryGetPropertyIgnoreCase(JsonElement element, string name, out JsonElement value)
        {
            if (element.ValueKind == JsonValueKind.Object)
            {
                foreach (var property in element.EnumerateObject())
                {
                    if (string.Equals(property.Name, name, StringComparison.OrdinalIgnoreCase))
                    {
                        value = property.Value;
                        return true;
                    }
                }
            }

            value = default(JsonElement);
            return false;
        }

        /// <summary>
        /// Resolves the configured operator email via <c>POST api/session/resolve</c> and
        /// records the outcome. Returns the resolution result, or <c>null</c> when no
        /// validation could be performed (no API client available in this composition).
        /// </summary>
        public async Task<SessionResolveResultDto> ValidateAsync()
        {
            string email = OperatorEmail?.Trim() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(email))
            {
                var blankResult = SessionResolveResultDto.Fail(
                    0,
                    "Google Email Operator belum dikonfigurasi. " + DefaultRefusalMessage);
                SetResult(blankResult);
                return blankResult;
            }

            if (_apiClient == null)
            {
                // Validation unavailable; do not block uploads (P1-S03 upload path preserved).
                SetResult(null);
                return null;
            }

            SessionResolveResultDto result;
            try
            {
                result = await _apiClient.ResolveSessionAsync(email);
            }
            catch (Exception ex)
            {
                result = SessionResolveResultDto.Fail(0, ex.Message);
            }

            SetResult(result);
            return result;
        }

        /// <summary>
        /// Clears the cached resolution outcome (e.g. after the operator configuration changes).
        /// </summary>
        public void Invalidate()
        {
            SetResult(null);
        }

        private void SetResult(SessionResolveResultDto result)
        {
            lock (_syncRoot)
            {
                _lastResult = result;
            }
        }
    }
}
