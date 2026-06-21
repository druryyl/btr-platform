using System;
using System.Collections.Generic;

namespace btr.application.ReportingContext.DashboardSnapshotAgg.Models
{
    public class DashboardSalesForecastAggregateResult
    {
        public int PeriodYear { get; set; }

        public int PeriodMonth { get; set; }

        public DateTime GeneratedAt { get; set; }

        public DateTime BusinessDate { get; set; }

        public int DaysInMonth { get; set; }

        public int DaysElapsed { get; set; }

        public int DaysRemaining { get; set; }

        public decimal CurrentSales { get; set; }

        public decimal TotalTarget { get; set; }

        public decimal? CurrentAchievementPercent { get; set; }

        public decimal DailyAverageSales { get; set; }

        public decimal ForecastSales { get; set; }

        public decimal? ForecastAchievementPercent { get; set; }

        public decimal? RequiredDailySales { get; set; }

        public decimal TargetGap { get; set; }

        public decimal ForecastVariance { get; set; }

        public decimal BestCaseSales { get; set; }

        public decimal WorstCaseSales { get; set; }

        public string ForecastConfidence { get; set; }

        public string ForecastRiskBand { get; set; }

        public List<DashboardSalesDailyPaceRow> DailyPace { get; set; }
            = new List<DashboardSalesDailyPaceRow>();
    }

    public class DashboardSalesDailyPaceRow
    {
        public DateTime PaceDate { get; set; }

        public int DayOfMonth { get; set; }

        public bool IsElapsed { get; set; }

        public decimal ActualAmount { get; set; }

        public decimal ProjectedDailyAmount { get; set; }
    }
}
