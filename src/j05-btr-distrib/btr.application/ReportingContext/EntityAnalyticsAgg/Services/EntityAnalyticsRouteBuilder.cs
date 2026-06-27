using btr.application.ReportingContext.EntityAnalyticsAgg.Contracts;

namespace btr.application.ReportingContext.EntityAnalyticsAgg.Services
{
    public static class EntityAnalyticsRouteBuilder
    {
        public static string BuildProfileRoute(
            IEntityTypeRegistry entityTypes,
            string entityType,
            string entityId)
        {
            if (string.IsNullOrWhiteSpace(entityType) || string.IsNullOrWhiteSpace(entityId))
                return null;

            if (entityTypes.TryGet(entityType, out var registration)
                && !string.IsNullOrWhiteSpace(registration.ProfileRouteTemplate))
            {
                return registration.ProfileRouteTemplate.Replace("{id}", entityId.Trim());
            }

            return $"/analytics/{entityType}/{entityId.Trim()}";
        }
    }
}
