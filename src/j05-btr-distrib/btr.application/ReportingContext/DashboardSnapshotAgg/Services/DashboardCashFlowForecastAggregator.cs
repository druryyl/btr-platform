using System;
using System.Collections.Generic;
using System.Linq;
using btr.application.FinanceContext.PiutangAgg.Contracts;
using btr.application.FinanceContext.PiutangAgg.Services;
using btr.application.ReportingContext.DashboardSnapshotAgg.Contracts;
using btr.application.ReportingContext.DashboardSnapshotAgg.Models;
using btr.application.SalesContext.FakturInfo;
using btr.nuna.Domain;

namespace btr.application.ReportingContext.DashboardSnapshotAgg.Services
{
    public class DashboardCashFlowForecastAggregator
    {
        public DashboardCashFlowForecastAggregateResult Aggregate(
            IEnumerable<PenerimaanPelunasanSalesDto> pelunasanRows,
            IEnumerable<FakturView> fakturRows,
            IEnumerable<PiutangOpenBalanceDto> openBalanceRows,
            IEnumerable<PiutangOpenBalanceWithSalesmanDto> openBalanceWithSalesmanRows,
            IEnumerable<PiutangOpenBalanceWithWilayahDto> openBalanceWithWilayahRows,
            DashboardCollectionAggregateResult collectionResult,
            Periode periode,
            DateTime businessDate,
            DateTime generatedAt,
            decimal largeDueSoonFloorAmount,
            IEnumerable<PenerimaanPelunasanSalesDto> pelunasanLookback30Rows = null)
        {
            if (periode is null)
                throw new ArgumentNullException(nameof(periode));
            if (collectionResult is null)
                throw new ArgumentNullException(nameof(collectionResult));

            var periodStart = periode.Tgl1.Date;
            var periodEnd = periode.Tgl2.Date;
            var asOfDate = businessDate.Date;

            if (asOfDate < periodStart)
                asOfDate = periodStart;
            if (asOfDate > periodEnd)
                asOfDate = periodEnd;

            var pelunasanList = (pelunasanRows ?? Enumerable.Empty<PenerimaanPelunasanSalesDto>()).ToList();
            var fakturList = (fakturRows ?? Enumerable.Empty<FakturView>()).ToList();
            var openList = (openBalanceRows ?? Enumerable.Empty<PiutangOpenBalanceDto>()).ToList();

            var cashCollectedMtd = collectionResult.CashCollectedMtd;
            var monthCollections = collectionResult.MonthCollections;
            var monthFakturOmzet = collectionResult.MonthFakturOmzet;

            var outstandingDueRemaining = openList
                .Where(r => r.KurangBayar > 1)
                .Where(r => r.JatuhTempo.Date > asOfDate && r.JatuhTempo.Date <= periodEnd)
                .Sum(r => r.KurangBayar);

            var calculation = CashFlowForecastPolicy.Compute(
                cashCollectedMtd,
                monthCollections,
                monthFakturOmzet,
                asOfDate,
                periodStart,
                periodEnd,
                outstandingDueRemaining);

            var dayBuckets = CollectionDayGrouper.BuildBuckets(periode);
            var dailyCashTotals = dayBuckets.ToDictionary(b => b.PaceDate, _ => 0m);
            var dailyCollectionTotals = dayBuckets.ToDictionary(b => b.PaceDate, _ => 0m);
            var dailyBillingTotals = dayBuckets.ToDictionary(b => b.PaceDate, _ => 0m);

            foreach (var row in pelunasanList)
            {
                var paymentDate = row.LunasDate.Date;
                if (paymentDate < periodStart || paymentDate > periodEnd)
                    continue;

                if (dailyCashTotals.ContainsKey(paymentDate))
                {
                    dailyCashTotals[paymentDate] += row.BayarTunai;
                    dailyCollectionTotals[paymentDate] += row.TotalBayar;
                }
            }

            foreach (var row in fakturList)
            {
                var fakturDate = row.Tgl.Date;
                if (fakturDate < periodStart || fakturDate > periodEnd)
                    continue;

                if (dailyBillingTotals.ContainsKey(fakturDate))
                    dailyBillingTotals[fakturDate] += row.GrandTotal;
            }

            var recent7CashAvg = ComputeRecent7DayAverage(
                dailyCashTotals,
                asOfDate,
                periodStart,
                calculation.DaysElapsed,
                calculation.DailyCashCollectionAverage);

            var bestCase = CashFlowForecastPolicy.ComputeBestCaseCash(
                calculation.DailyCashCollectionAverage,
                recent7CashAvg,
                calculation.DaysInMonth);

            var worstCase = CashFlowForecastPolicy.ComputeWorstCaseCash(
                calculation.DailyCashCollectionAverage,
                recent7CashAvg,
                calculation.DaysInMonth);

            var dailyPace = dayBuckets.Select(bucket =>
            {
                var isElapsed = bucket.PaceDate <= asOfDate;
                return new DashboardCashFlowDailyPaceRow
                {
                    PaceDate = bucket.PaceDate,
                    DayOfMonth = bucket.DayOfMonth,
                    IsElapsed = isElapsed,
                    ActualCashAmount = isElapsed ? dailyCashTotals[bucket.PaceDate] : 0m,
                    ActualCollectionAmount = isElapsed ? dailyCollectionTotals[bucket.PaceDate] : 0m,
                    ProjectedDailyCashAmount = calculation.DailyCashCollectionAverage
                };
            }).ToList();

            decimal cumulativeCollections = 0m;
            decimal cumulativeBilling = 0m;
            var recoveryTrend = dayBuckets.Select(bucket =>
            {
                cumulativeCollections += dailyCollectionTotals[bucket.PaceDate];
                cumulativeBilling += dailyBillingTotals[bucket.PaceDate];
                return new DashboardCashFlowRecoveryTrendRow
                {
                    TrendDate = bucket.PaceDate,
                    DayOfMonth = bucket.DayOfMonth,
                    IsElapsed = bucket.PaceDate <= asOfDate,
                    CumulativeCollections = cumulativeCollections,
                    CumulativeBilling = cumulativeBilling
                };
            }).ToList();

            var collectionRisks = CashFlowCollectionRiskBuilder.Build(
                openList,
                openBalanceWithSalesmanRows,
                openBalanceWithWilayahRows,
                collectionResult,
                pelunasanLookback30Rows,
                asOfDate,
                periodEnd,
                largeDueSoonFloorAmount);

            return new DashboardCashFlowForecastAggregateResult
            {
                PeriodYear = periodStart.Year,
                PeriodMonth = periodStart.Month,
                GeneratedAt = generatedAt,
                BusinessDate = asOfDate,
                DaysInMonth = calculation.DaysInMonth,
                DaysElapsed = calculation.DaysElapsed,
                DaysRemaining = calculation.DaysRemaining,
                CashCollectedMtd = cashCollectedMtd,
                MonthCollections = monthCollections,
                MonthFakturOmzet = monthFakturOmzet,
                DailyCashCollectionAverage = calculation.DailyCashCollectionAverage,
                DailyCollectionAverage = calculation.DailyCollectionAverage,
                ExpectedCashCollection = calculation.ExpectedCashCollection,
                ProjectedMonthEndTotalCollections = calculation.ProjectedMonthEndTotalCollections,
                CollectionForecastPercent = calculation.CollectionForecastPercent,
                RecoveryVsBillingPercent = collectionResult.RecoveryVsBillingPercent,
                RecoveryVsBillingForecastPercent = calculation.RecoveryVsBillingForecastPercent,
                RemainingCollectionTarget = calculation.RemainingCollectionTarget,
                RequiredDailyCollection = calculation.RequiredDailyCollection,
                OutstandingDueRemaining = outstandingDueRemaining,
                OverdueOutstanding = collectionResult.OverdueExposure,
                CollectionGap = calculation.CollectionGap,
                ForecastVarianceCash = calculation.ForecastVarianceCash,
                ExpectedCollectionRatePercent = calculation.ExpectedCollectionRatePercent,
                BestCaseCash = bestCase,
                WorstCaseCash = worstCase,
                ForecastConfidence = calculation.ForecastConfidence,
                ForecastRiskBand = calculation.ForecastRiskBand,
                DailyPace = dailyPace,
                RecoveryTrend = recoveryTrend,
                CollectionRisks = collectionRisks
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
