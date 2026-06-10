using System;

namespace btr.application.SalesContext.VisitPlanAgg.UseCases
{
    public class RegenerateVisitPlanRequest
    {
        public string SalesPersonId { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public string TriggeredBy { get; set; } = "Manual";
    }
}
