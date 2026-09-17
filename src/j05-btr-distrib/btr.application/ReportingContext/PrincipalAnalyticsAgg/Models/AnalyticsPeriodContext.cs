using System;

namespace btr.application.ReportingContext.PrincipalAnalyticsAgg.Models
{
    public class AnalyticsPeriodContext
    {
        public DateTime AsOfDate { get; set; }

        public int PeriodYear { get; set; }

        public int PeriodMonth { get; set; }

        public DateTime MonthStart { get; set; }

        public DateTime MonthEnd { get; set; }

        public int ElapsedDays { get; set; }

        public int DaysInMonth { get; set; }

        public int PriorYear { get; set; }

        public int PriorMonth { get; set; }

        public DateTime PriorYearStart { get; set; }

        public DateTime PriorYearEnd { get; set; }

        public DateTime GeneratedAt { get; set; }
    }
}
