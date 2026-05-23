using System;

namespace btr.domain.SalesContext.SalesOmzetHealthWeeklyAgg
{
    public class SalesOmzetHealthWeeklyModel : ISalesOmzetHealthWeeklyKey
    {
        public SalesOmzetHealthWeeklyModel()
        {
        }

        public SalesOmzetHealthWeeklyModel(string healthWeeklyId) => HealthWeeklyId = healthWeeklyId;

        public string HealthWeeklyId { get; set; }
        public int YearNumber { get; set; }
        public int WeekNumber { get; set; }
        public DateTime PeriodStartDate { get; set; }
        public DateTime PeriodEndDate { get; set; }
        public SalesOmzetHealthLevelEnum HealthLevel { get; set; }
        public int HealthScore { get; set; }
        public int MissingOrdersCount { get; set; }
        public int MissingFaktursCount { get; set; }
        public int UnlinkedFaktursCount { get; set; }
        public int StaleDataCount { get; set; }
        public DateTime LastCalculatedAt { get; set; }
        public int CalculationDurationMs { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
