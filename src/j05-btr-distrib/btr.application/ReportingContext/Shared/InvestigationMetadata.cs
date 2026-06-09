using System.Collections.Generic;

namespace btr.application.ReportingContext.Shared
{
    public class InvestigationMetadata
    {
        public string SignalKey { get; set; }

        public string SignalLabel { get; set; }

        public string EntityType { get; set; }

        public string EntityId { get; set; }

        public string EntityName { get; set; }

        public string DashboardRoute { get; set; }

        public string ReportRoute { get; set; }

        public InvestigationSuggestedQuery SuggestedQuery { get; set; }

        public IList<InvestigationStep> InvestigationSteps { get; set; }

        public string DesktopNextStep { get; set; }
    }

    public class InvestigationSuggestedQuery
    {
        public string FreeText { get; set; }

        public string CustomerId { get; set; }

        public string SalesmanId { get; set; }

        public string BrgId { get; set; }

        public string WarehouseId { get; set; }

        public string SupplierId { get; set; }

        public string PeriodMode { get; set; }

        public string PostingFilter { get; set; }
    }

    public class InvestigationStep
    {
        public int Order { get; set; }

        public string Label { get; set; }

        public string ReportRoute { get; set; }

        public string DashboardRoute { get; set; }

        public InvestigationSuggestedQuery SuggestedQuery { get; set; }
    }
}
