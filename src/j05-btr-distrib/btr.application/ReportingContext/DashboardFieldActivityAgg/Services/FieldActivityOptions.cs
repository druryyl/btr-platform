using System;

namespace btr.application.ReportingContext.DashboardFieldActivityAgg.Services
{
    public class FieldActivityOptions
    {
        public const string SECTION_NAME = "FieldActivity";

        public DateTime VisitPlanGoLiveDate { get; set; } = new DateTime(2026, 3, 1);
    }
}
