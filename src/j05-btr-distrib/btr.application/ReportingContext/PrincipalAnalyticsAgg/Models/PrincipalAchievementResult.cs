using System;
using System.Collections.Generic;

namespace btr.application.ReportingContext.PrincipalAnalyticsAgg.Models
{
    public class PrincipalAchievementResult
    {
        public string AchievementAmountKpiId { get; set; }

        public string AchievementPercentageKpiId { get; set; }

        public string SalesOutKpiId { get; set; }

        public string TargetKpiId { get; set; }

        public int PeriodYear { get; set; }

        public int PeriodMonth { get; set; }

        public DateTime GeneratedAt { get; set; }

        public IList<PrincipalAchievementRow> Principals { get; set; }
            = new List<PrincipalAchievementRow>();
    }

    public class PrincipalAchievementRow
    {
        public string AchievementAmountKpiId { get; set; }

        public string AchievementPercentageKpiId { get; set; }

        public string SalesOutKpiId { get; set; }

        public string TargetKpiId { get; set; }

        public string SupplierId { get; set; }

        public string SupplierName { get; set; }

        public decimal? SalesOutAmount { get; set; }

        public decimal? TargetAmount { get; set; }

        public decimal? AchievementAmount { get; set; }

        public decimal? AchievementPercentage { get; set; }

        public int SortOrder { get; set; }
    }
}
