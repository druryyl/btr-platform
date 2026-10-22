using System;
using System.Text.Json;
using System.Threading.Tasks;
using BtrReturGudang.AppTier.Contracts;
using BtrReturGudang.AppTier.DTOs;

namespace BtrReturGudang.Wpf.Services
{
    /// <summary>
    /// Presentation-layer operator session validation gate for the workstation.
    /// Resolves the configured operator Google email through the Cloud anonymous
    /// endpoint POST api/session/resolve and caches the last outcome so upload
    /// actions can be pre-empted with the Cloud refusal message instead of an opaque HTTP 409.
    /// Traceable to: ARCHITECTURE §4 TD-012, §5.2 SCR-07.
    /// </summary>
    public class OperatorSessionValidator
    {
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

        public string OperatorEmail => _registryHelper?.LastOperatorEmail ?? string.Empty;

        public SessionResolveResultDto LastResult
        {
            get { lock (_syncRoot) { return _lastResult; } }
        }

        public bool HasBeenValidated
        {
            get { lock (_syncRoot) { return _lastResult != null; } }
        }

        public bool IsResolved
        {
            get { lock (_syncRoot) { return _lastResult != null && _lastResult.Success; } }
        }

        public bool IsUnresolved
        {
            get
            {
                lock (_syncRoot)
                {
                    if (_lastResult == null) return false;
                    if (_lastResult.Success) return false;
                    if (string.IsNullOrWhiteSpace(OperatorEmail)) return true;
                    return IsDefiniteRefusal(_lastResult.StatusCode);
                }
            }
        }

        private static bool IsDefiniteRefusal(int statusCode)
        {
            return statusCode == 400 || statusCode == 403 || statusCode == 409;
        }

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

        public static string ExtractRefusalMessage(SessionResolveResultDto result)
        {
            if (result == null) return DefaultRefusalMessage;

            foreach (var candidate in new[] { result.RawResponse, result.ErrorMessage })
            {
                if (string.IsNullOrWhiteSpace(candidate)) continue;

                var parsed = ExtractJsEndMessage(candidate);
                if (!string.IsNullOrWhiteSpace(parsed)) return parsed.Trim();

                if (LooksLikeJsonObject(candidate)) continue;

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

                    if (TryGetPropertyIgnoreCase(root, "data", out var data))
                    {
                        var fromData = ReadMessage(data);
                        if (!string.IsNullOrWhiteSpace(fromData)) return fromData;
                    }

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
