using System;
using System.Collections.Generic;

namespace btr.application.ReportingContext.PrincipalAnalyticsAgg.Models
{
    public class PrincipalPacingAchievementResult
    {
        public string PacingAchievementKpiId { get; set; }

        public string SalesOutKpiId { get; set; }

        public string TargetKpiId { get; set; }

        public int PeriodYear { get; set; }

        public int PeriodMonth { get; set; }

        public DateTime GeneratedAt { get; set; }

        public IList<PrincipalPacingAchievementRow> Principals { get; set; }
            = new List<PrincipalPacingAchievementRow>();
    }

    public class PrincipalPacingAchievementRow
    {
        public string PacingAchievementKpiId { get; set; }

        public string SalesOutKpiId { get; set; }

        public string TargetKpiId { get; set; }

        public string SupplierId { get; set; }

        public string SupplierName { get; set; }

        public decimal? SalesOutAmount { get; set; }

        public decimal? TargetAmount { get; set; }

        public decimal? PacingAchievementPercentage { get; set; }

        public int SortOrder { get; set; }
    }
}