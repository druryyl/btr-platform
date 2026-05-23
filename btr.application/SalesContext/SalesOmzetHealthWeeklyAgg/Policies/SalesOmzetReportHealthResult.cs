using System.Collections.Generic;
using btr.domain.SalesContext.SalesOmzetHealthWeeklyAgg;

namespace btr.application.SalesContext.SalesOmzetHealthWeeklyAgg.Policies
{
    public class SalesOmzetReportHealthResult
    {
        public SalesOmzetHealthLevelEnum FinalLevel { get; set; }
        public int? AverageScore { get; set; }
        public IReadOnlyList<SalesOmzetReportHealthWeekDetail> WeekDetails { get; set; }
    }

    public class SalesOmzetReportHealthWeekDetail
    {
        public int YearNumber { get; set; }
        public int WeekNumber { get; set; }
        public SalesOmzetHealthLevelEnum Level { get; set; }
        public int? HealthScore { get; set; }
        public bool IsCalculated { get; set; }
    }
}
