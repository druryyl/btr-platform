using System;

namespace btr.application.ReportingContext.DashboardSnapshotAgg.Services
{
    public sealed class InventoryForecastCalculation
    {
        public decimal Adc30 { get; set; }

        public decimal Adc90 { get; set; }

        public decimal AdcUsed { get; set; }

        public decimal? DaysOfSupply { get; set; }

        public DateTime? ProjectedStockOutDate { get; set; }

        public DateTime? ReorderDate { get; set; }

        public decimal RecommendedPurchaseQty { get; set; }

        public decimal ForecastQtyAtHorizon { get; set; }

        public decimal ForecastValueAtHorizon { get; set; }

        public decimal BestCaseForecastValue { get; set; }

        public decimal WorstCaseForecastValue { get; set; }

        public string DosSeverity { get; set; }

        public string PurchaseUrgency { get; set; }

        public bool IsInsufficientHistory { get; set; }
    }

    public static class InventoryForecastPolicy
    {
        public const string ConfidenceLow = "Low";
        public const string ConfidenceMedium = "Medium";
        public const string ConfidenceHigh = "High";

        public const string SeverityCritical = "Critical";
        public const string SeverityWarning = "Warning";
        public const string SeverityNormal = "Normal";

        public const string UrgencyCritical = "Critical";
        public const string UrgencyHigh = "High";
        public const string UrgencyMedium = "Medium";
        public const string UrgencyLow = "Low";

        public const int AdcWindow30Days = 30;
        public const int AdcWindow90Days = 90;

        public static decimal ComputeAdc30(decimal soldQty30) =>
            Math.Round(soldQty30 / AdcWindow30Days, 4, MidpointRounding.AwayFromZero);

        public static decimal ComputeAdc90(decimal soldQty90) =>
            Math.Round(soldQty90 / AdcWindow90Days, 4, MidpointRounding.AwayFromZero);

        public static decimal? ComputeDaysOfSupply(decimal qty, decimal adc)
        {
            if (adc <= 0)
                return null;

            return Math.Round(qty / adc, 2, MidpointRounding.AwayFromZero);
        }

        public static DateTime? ComputeProjectedStockOutDate(DateTime businessDate, decimal? daysOfSupply)
        {
            if (!daysOfSupply.HasValue)
                return null;

            var days = (int)Math.Ceiling(daysOfSupply.Value);
            return businessDate.Date.AddDays(days);
        }

        public static DateTime? ComputeReorderDate(DateTime? stockOutDate, int leadTimeDays)
        {
            if (!stockOutDate.HasValue)
                return null;

            return stockOutDate.Value.Date.AddDays(-leadTimeDays);
        }

        public static decimal ComputeRecommendedPurchaseQty(
            decimal qty,
            decimal adc,
            int leadTimeDays,
            int coverageDays)
        {
            if (adc <= 0)
                return 0m;

            var target = adc * (leadTimeDays + coverageDays);
            var gap = target - qty;
            if (gap <= 0)
                return 0m;

            return Math.Ceiling(gap);
        }

        public static decimal ComputeForecastQtyAtHorizon(decimal qty, decimal adc, int horizonDays) =>
            Math.Max(0m, Math.Round(qty - adc * horizonDays, 4, MidpointRounding.AwayFromZero));

        public static decimal ComputeProjectedValue(decimal forecastQty, decimal unitHpp) =>
            Math.Round(forecastQty * unitHpp, 2, MidpointRounding.AwayFromZero);

        public static decimal ComputeBestCaseAdc(decimal adc30, decimal adc90) =>
            Math.Min(adc30, adc90);

        public static decimal ComputeWorstCaseAdc(decimal adc30, decimal adc90) =>
            Math.Max(adc30, adc90);

        public static string ResolveConfidence(int horizonDays, decimal companySoldQty30, int businessDayOfMonth)
        {
            if (companySoldQty30 <= 0 || businessDayOfMonth <= 5)
                return ConfidenceLow;

            if (horizonDays >= 21)
                return ConfidenceHigh;

            return ConfidenceMedium;
        }

        public static string ResolveDosSeverity(decimal? daysOfSupply, int criticalDays = 7, int warningDays = 14)
        {
            if (!daysOfSupply.HasValue)
                return SeverityNormal;

            if (daysOfSupply.Value <= criticalDays)
                return SeverityCritical;

            if (daysOfSupply.Value <= warningDays)
                return SeverityWarning;

            return SeverityNormal;
        }

        public static string ResolvePurchaseUrgency(
            decimal? daysOfSupply,
            DateTime? reorderDate,
            DateTime businessDate,
            int criticalDays = 7)
        {
            if (reorderDate.HasValue && reorderDate.Value.Date < businessDate.Date)
                return UrgencyCritical;

            if (daysOfSupply.HasValue && daysOfSupply.Value <= criticalDays)
                return UrgencyCritical;

            if (daysOfSupply.HasValue && daysOfSupply.Value <= 14)
                return UrgencyHigh;

            if (daysOfSupply.HasValue && daysOfSupply.Value <= 30)
                return UrgencyMedium;

            return UrgencyLow;
        }

        public static int ComputeHealthScore(
            decimal stockOutRiskPct,
            decimal overstockValuePct,
            decimal atRiskInventoryPct)
        {
            var penalty = stockOutRiskPct * 0.40m + overstockValuePct * 0.30m + atRiskInventoryPct * 0.30m;
            var score = (int)Math.Round(100m - penalty, MidpointRounding.AwayFromZero);
            return Math.Max(0, Math.Min(100, score));
        }

        public static InventoryForecastCalculation ComputeItem(
            decimal currentQty,
            decimal unitHpp,
            decimal soldQty30,
            decimal soldQty90,
            int planningHorizonDays,
            int defaultLeadTimeDays,
            int coverageDays,
            DateTime businessDate,
            DateTime? firstFakturDate = null,
            decimal soldQtyMtd = 0m,
            int daysElapsedInMonth = 1)
        {
            var adc30 = ComputeAdc30(soldQty30);
            var adc90 = ComputeAdc90(soldQty90);
            var adcUsed = adc30;
            var isInsufficientHistory = false;

            if (adc30 <= 0 && firstFakturDate.HasValue)
            {
                var daysSinceFirstSale = (businessDate.Date - firstFakturDate.Value.Date).Days;
                if (daysSinceFirstSale >= 0 && daysSinceFirstSale < 29)
                {
                    var elapsed = Math.Max(1, daysElapsedInMonth);
                    adcUsed = Math.Round(soldQtyMtd / elapsed, 4, MidpointRounding.AwayFromZero);
                    isInsufficientHistory = adcUsed <= 0;
                }
                else if (soldQty30 <= 0)
                {
                    isInsufficientHistory = true;
                }
            }

            var dos = ComputeDaysOfSupply(currentQty, adcUsed);
            var stockOutDate = adcUsed > 0 && dos.HasValue && dos.Value <= planningHorizonDays
                ? ComputeProjectedStockOutDate(businessDate, dos)
                : null;
            var reorderDate = ComputeReorderDate(stockOutDate, defaultLeadTimeDays);
            var recommendedQty = adcUsed > 0
                ? ComputeRecommendedPurchaseQty(currentQty, adcUsed, defaultLeadTimeDays, coverageDays)
                : 0m;
            var forecastQty = ComputeForecastQtyAtHorizon(currentQty, adcUsed, planningHorizonDays);
            var forecastValue = ComputeProjectedValue(forecastQty, unitHpp);

            var bestAdc = ComputeBestCaseAdc(adc30, adc90);
            var worstAdc = ComputeWorstCaseAdc(adc30, adc90);
            var bestQty = ComputeForecastQtyAtHorizon(currentQty, bestAdc, planningHorizonDays);
            var worstQty = ComputeForecastQtyAtHorizon(currentQty, worstAdc, planningHorizonDays);

            return new InventoryForecastCalculation
            {
                Adc30 = adc30,
                Adc90 = adc90,
                AdcUsed = adcUsed,
                DaysOfSupply = dos,
                ProjectedStockOutDate = stockOutDate,
                ReorderDate = reorderDate,
                RecommendedPurchaseQty = recommendedQty,
                ForecastQtyAtHorizon = forecastQty,
                ForecastValueAtHorizon = forecastValue,
                BestCaseForecastValue = ComputeProjectedValue(bestQty, unitHpp),
                WorstCaseForecastValue = ComputeProjectedValue(worstQty, unitHpp),
                DosSeverity = ResolveDosSeverity(dos),
                PurchaseUrgency = ResolvePurchaseUrgency(dos, reorderDate, businessDate),
                IsInsufficientHistory = isInsufficientHistory
            };
        }
    }
}
