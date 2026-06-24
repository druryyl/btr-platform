using System;
using System.Collections.Generic;
using btr.application.ReportingContext.EntityAnalyticsAgg.Contracts;

namespace btr.application.ReportingContext.EntityAnalyticsAgg.Services
{
    public class EntityAnalyticsDimensionLabelRegistry : IDimensionLabelRegistry
    {
        private readonly Dictionary<string, Dictionary<string, string>> _labelsByType
            = new Dictionary<string, Dictionary<string, string>>(StringComparer.OrdinalIgnoreCase);

        public void Register(string entityType, string dimensionKpiId, string displayLabel)
        {
            if (string.IsNullOrWhiteSpace(entityType))
                throw new ArgumentException("EntityType is required.", nameof(entityType));
            if (string.IsNullOrWhiteSpace(dimensionKpiId))
                throw new ArgumentException("DimensionKpiId is required.", nameof(dimensionKpiId));

            if (!_labelsByType.TryGetValue(entityType, out var labels))
            {
                labels = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                _labelsByType[entityType] = labels;
            }

            labels[dimensionKpiId] = displayLabel ?? dimensionKpiId;
        }

        public bool TryGetLabel(string entityType, string dimensionKpiId, out string displayLabel)
        {
            displayLabel = null;
            if (string.IsNullOrWhiteSpace(entityType) || string.IsNullOrWhiteSpace(dimensionKpiId))
                return false;

            return _labelsByType.TryGetValue(entityType, out var labels)
                && labels.TryGetValue(dimensionKpiId, out displayLabel);
        }
    }
}
