namespace btr.application.ReportingContext.EntityAnalyticsAgg.Contracts
{
    /// <summary>
    /// Extensibility hook for registering entity types, KPI packs, and dimension labels without modifying platform code.
    /// </summary>
    public interface IEntityAnalyticsRegistrar
    {
        void Register(
            IEntityTypeRegistry entityTypes,
            IKpiRegistry kpiRegistry,
            IDimensionLabelRegistry dimensionLabels);
    }
}
