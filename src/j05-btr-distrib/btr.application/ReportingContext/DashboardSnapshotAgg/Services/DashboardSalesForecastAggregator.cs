using System;
using System.Collections.Generic;
using System.Linq;
using btr.application.ReportingContext.DashboardSnapshotAgg.Models;
using btr.application.SalesContext.FakturInfo;
using btr.application.SalesContext.SalesOmzetAgg.Services;
using btr.nuna.Domain;

namespace btr.application.ReportingContext.DashboardSnapshotAgg.Services
{
    public class DashboardSalesForecastAggregator
    {
        public DashboardSalesForecastAggregateResult Aggregate(
            IEnumerable<FakturView> rows,
            Periode periode,
            decimal totalTarget,
            DateTime businessDate,
            DateTime generatedAt)
        {
            if (periode is null)
                throw new ArgumentNullException(nameof(periode));

            var list = (rows ?? Enumerable.Empty<FakturView>()).ToList();
            var periodStart = periode.Tgl1.Date;
            var periodEnd = periode.Tgl2.Date;
            var asOfDate = businessDate.Date;

            if (asOfDate < periodStart)
                asOfDate = periodStart;
            if (asOfDate > periodEnd)
                asOfDate = periodEnd;

            var currentSales = list.Sum(r => r.GrandTotal);
            var target = totalTarget > 0 ? (decimal?)totalTarget : null;

            var calculation = SalesForecastPolicy.Compute(
                currentSales,
                target,
                asOfDate,
                periodStart,
                periodEnd);

            var dayBuckets = SalesOmzetChartDayGrouper.BuildBuckets(periode);
            var dailyTotals = dayBuckets.ToDictionary(
                b => b.PaceDate,
                b => 0m);

            foreach (var row in list)
            {
                var fakturDate = row.Tgl.Date;
                if (fakturDate < periodStart || fakturDate > periodEnd)
                    continue;

                if (dailyTotals.ContainsKey(fakturDate))
                    dailyTotals[fakturDate] += row.GrandTotal;
            }

            var recent7Avg = ComputeRecent7DayAverage(
                dailyTotals,
                asOfDate,
                periodStart,
                calculation.DaysElapsed,
                calculation.DailyAverageSales);
            var bestCase = SalesForecastPolicy.ComputeBestCase(
                calculation.DailyAverageSales,
                recent7Avg,
                calculation.DaysInMonth);
            var worstCase = SalesForecastPolicy.ComputeWorstCase(
                calculation.DailyAverageSales,
                recent7Avg,
                calculation.DaysInMonth);

            var dailyPace = dayBuckets.Select(bucket =>
            {
                var isElapsed = bucket.PaceDate <= asOfDate;
                return new DashboardSalesDailyPaceRow
                {
                    PaceDate = bucket.PaceDate,
                    DayOfMonth = bucket.DayOfMonth,
                    IsElapsed = isElapsed,
                    ActualAmount = isElapsed ? dailyTotals[bucket.PaceDate] : 0m,
                    ProjectedDailyAmount = calculation.DailyAverageSales
                };
            }).ToList();

            return new DashboardSalesForecastAggregateResult
            {
                PeriodYear = periodStart.Year,
                PeriodMonth = periodStart.Month,
                GeneratedAt = generatedAt,
                BusinessDate = asOfDate,
                DaysInMonth = calculation.DaysInMonth,
                DaysElapsed = calculation.DaysElapsed,
                DaysRemaining = calculation.DaysRemaining,
                CurrentSales = currentSales,
                TotalTarget = totalTarget,
                CurrentAchievementPercent = calculation.CurrentAchievementPercent,
                DailyAverageSales = calculation.DailyAverageSales,
                ForecastSales = calculation.ForecastSales,
                ForecastAchievementPercent = calculation.ForecastAchievementPercent,
                RequiredDailySales = calculation.RequiredDailySales,
                TargetGap = calculation.TargetGap,
                ForecastVariance = calculation.ForecastVariance,
                BestCaseSales = bestCase,
                WorstCaseSales = worstCase,
                ForecastConfidence = calculation.ForecastConfidence,
                ForecastRiskBand = calculation.ForecastRiskBand,
                DailyPace = dailyPace
            };
        }

        private static decimal ComputeRecent7DayAverage(
            Dictionary<DateTime, decimal> dailyTotals,
            DateTime businessDate,
            DateTime periodStart,
            int daysElapsed,
            decimal mtdDailyAverage)
        {
            if (daysElapsed < 7)
                return mtdDailyAverage;

            var windowStart = businessDate.AddDays(-6);
            if (windowStart < periodStart)
                windowStart = periodStart;

            var sum = 0m;
            for (var cursor = windowStart; cursor <= businessDate; cursor = cursor.AddDays(1))
            {
                if (dailyTotals.TryGetValue(cursor.Date, out var amount))
                    sum += amount;
            }

            return Math.Round(sum / 7m, 2, MidpointRounding.AwayFromZero);
        }
    }
}
