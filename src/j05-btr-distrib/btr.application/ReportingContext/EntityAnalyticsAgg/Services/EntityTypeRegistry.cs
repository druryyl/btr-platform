using System;
using System.Collections.Generic;
using System.Linq;
using btr.application.ReportingContext.EntityAnalyticsAgg.Contracts;
using btr.application.ReportingContext.EntityAnalyticsAgg.Models;

namespace btr.application.ReportingContext.EntityAnalyticsAgg.Services
{
    public class EntityTypeRegistry : IEntityTypeRegistry
    {
        private readonly Dictionary<string, EntityTypeRegistration> _byCode
            = new Dictionary<string, EntityTypeRegistration>(StringComparer.OrdinalIgnoreCase);

        public void Register(EntityTypeRegistration registration)
        {
            if (registration is null)
                throw new ArgumentNullException(nameof(registration));
            if (string.IsNullOrWhiteSpace(registration.EntityTypeCode))
                throw new ArgumentException("EntityTypeCode is required.", nameof(registration));

            _byCode[registration.EntityTypeCode] = registration;
        }

        public bool TryGet(string entityTypeCode, out EntityTypeRegistration registration)
        {
            registration = null;
            if (string.IsNullOrWhiteSpace(entityTypeCode))
                return false;

            return _byCode.TryGetValue(entityTypeCode, out registration);
        }

        public bool IsRegistered(string entityTypeCode)
        {
            return TryGet(entityTypeCode, out _);
        }

        public string NormalizeEntityTypeCode(string entityTypeCode)
        {
            if (!TryGet(entityTypeCode, out var registration))
                return null;

            return registration.EntityTypeCode;
        }

        public IReadOnlyList<EntityTypeRegistration> GetAll()
        {
            return _byCode.Values
                .OrderBy(r => r.EntityTypeCode, StringComparer.OrdinalIgnoreCase)
                .ToList();
        }
    }
}
