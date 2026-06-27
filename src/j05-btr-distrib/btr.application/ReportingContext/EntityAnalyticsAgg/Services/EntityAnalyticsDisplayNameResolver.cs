using System;

namespace btr.application.ReportingContext.EntityAnalyticsAgg.Services
{
    public static class EntityAnalyticsDisplayNameResolver
    {
        public static string PreferBusinessName(string candidate, string lookupName, string codeOrId)
        {
            var trimmedCandidate = candidate?.Trim();
            var trimmedLookup = lookupName?.Trim();
            var trimmedCode = codeOrId?.Trim() ?? string.Empty;

            if (!string.IsNullOrWhiteSpace(trimmedLookup)
                && !LooksLikeIdentifier(trimmedLookup, trimmedCode))
            {
                if (string.IsNullOrWhiteSpace(trimmedCandidate)
                    || LooksLikeIdentifier(trimmedCandidate, trimmedCode))
                {
                    return trimmedLookup;
                }
            }

            if (!string.IsNullOrWhiteSpace(trimmedCandidate))
                return trimmedCandidate;

            if (!string.IsNullOrWhiteSpace(trimmedLookup))
                return trimmedLookup;

            return trimmedCode;
        }

        public static string ResolveStoredOrLookup(
            string storedDisplayName,
            string entityCode,
            string entityId,
            string lookupDisplayName)
        {
            var stored = storedDisplayName?.Trim();
            var code = entityCode?.Trim();
            var id = entityId?.Trim();

            if (!string.IsNullOrWhiteSpace(stored)
                && !LooksLikeIdentifier(stored, code)
                && !LooksLikeIdentifier(stored, id))
            {
                return stored;
            }

            var resolved = lookupDisplayName?.Trim();
            if (!string.IsNullOrWhiteSpace(resolved)
                && !LooksLikeIdentifier(resolved, code)
                && !LooksLikeIdentifier(resolved, id))
            {
                return resolved;
            }

            return stored ?? resolved ?? code ?? id ?? string.Empty;
        }

        public static bool LooksLikeIdentifier(string value, string identifier)
        {
            return !string.IsNullOrWhiteSpace(value)
                && !string.IsNullOrWhiteSpace(identifier)
                && string.Equals(value.Trim(), identifier.Trim(), StringComparison.OrdinalIgnoreCase);
        }
    }
}
