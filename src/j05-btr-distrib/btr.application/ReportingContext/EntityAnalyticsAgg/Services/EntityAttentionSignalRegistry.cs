using System;
using System.Collections.Generic;
using btr.application.ReportingContext.EntityAnalyticsAgg.Contracts;
using btr.application.ReportingContext.EntityAnalyticsAgg.Models;

namespace btr.application.ReportingContext.EntityAnalyticsAgg.Services
{
    public class EntityAttentionSignalRegistry : IAttentionSignalRegistry
    {
        private readonly Dictionary<string, Dictionary<string, AttentionSignalDefinition>> _definitions =
            new Dictionary<string, Dictionary<string, AttentionSignalDefinition>>(StringComparer.OrdinalIgnoreCase);

        public void Register(string entityType, AttentionSignalDefinition definition)
        {
            if (string.IsNullOrWhiteSpace(entityType) || definition == null
                || string.IsNullOrWhiteSpace(definition.SignalCode))
            {
                return;
            }

            if (!_definitions.TryGetValue(entityType, out var byCode))
            {
                byCode = new Dictionary<string, AttentionSignalDefinition>(StringComparer.OrdinalIgnoreCase);
                _definitions[entityType] = byCode;
            }

            byCode[definition.SignalCode.Trim()] = definition;
        }

        public bool TryResolve(string entityType, string signalCode, out AttentionSignalDefinition definition)
        {
            definition = null;
            if (string.IsNullOrWhiteSpace(entityType) || string.IsNullOrWhiteSpace(signalCode))
                return false;

            return _definitions.TryGetValue(entityType, out var byCode)
                && byCode.TryGetValue(signalCode.Trim(), out definition);
        }
    }
}
