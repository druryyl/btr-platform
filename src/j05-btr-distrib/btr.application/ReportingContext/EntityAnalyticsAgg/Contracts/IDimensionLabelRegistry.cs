namespace btr.application.ReportingContext.EntityAnalyticsAgg.Contracts
{
    /// <summary>
    /// Maps EA-DIM-* snapshot row keys to management-facing dimension labels per entity type.
    /// Entity registrars populate this at startup; infrastructure reads labels without entity-specific logic.
    /// </summary>
    public interface IDimensionLabelRegistry
    {
        void Register(string entityType, string dimensionKpiId, string displayLabel);

        bool TryGetLabel(string entityType, string dimensionKpiId, out string displayLabel);
    }
}
