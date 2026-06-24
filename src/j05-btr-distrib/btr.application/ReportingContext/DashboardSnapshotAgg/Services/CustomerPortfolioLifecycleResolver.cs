using System;
using System.Collections.Generic;
using System.Linq;
using btr.application.ReportingContext.DashboardSnapshotAgg.Models;

namespace btr.application.ReportingContext.DashboardSnapshotAgg.Services
{
    public static class CustomerPortfolioLifecycleResolver
    {
        public static string Resolve(
            bool hasPurchaseHistory,
            DateTime? firstPurchaseDate,
            DateTime? lastPurchaseDate,
            int? daysSinceLastFaktur,
            bool isActiveMtd,
            bool isDormant,
            CustomerRiskForecastContext forecast,
            decimal mtdOmzet,
            decimal priorMonthOmzet,
            DateTime businessDate,
            CustomerPortfolioOptions options)
        {
            options = options ?? CustomerPortfolioOptions.FromDashboardOptions(null);

            if (!hasPurchaseHistory)
                return CustomerPortfolioOptimizationPolicy.LifecycleNeverPurchased;

            if (isDormant)
                return CustomerPortfolioOptimizationPolicy.LifecycleDormant;

            if (firstPurchaseDate.HasValue &&
                firstPurchaseDate.Value.Date >= businessDate.Date.AddDays(-options.NewCustomerDaysThreshold))
            {
                return CustomerPortfolioOptimizationPolicy.LifecycleNew;
            }

            if (HasDecliningSignals(forecast))
                return CustomerPortfolioOptimizationPolicy.LifecycleDeclining;

            if (IsGrowing(forecast, mtdOmzet, priorMonthOmzet, firstPurchaseDate, businessDate, options))
                return CustomerPortfolioOptimizationPolicy.LifecycleGrowing;

            return CustomerPortfolioOptimizationPolicy.LifecycleMature;
        }

        private static bool HasDecliningSignals(CustomerRiskForecastContext forecast)
        {
            if (forecast?.Signals == null || forecast.Signals.Count == 0)
                return false;

            return forecast.Signals.Any(s =>
                string.Equals(s.SignalKey, CustomerRiskSignalBuilder.SignalModerateDecline, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(s.SignalKey, CustomerRiskSignalBuilder.SignalSevereDecline, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(s.SignalKey, CustomerRiskSignalBuilder.SignalStoppedAfterHistory, StringComparison.OrdinalIgnoreCase));
        }

        private static bool IsGrowing(
            CustomerRiskForecastContext forecast,
            decimal mtdOmzet,
            decimal priorMonthOmzet,
            DateTime? firstPurchaseDate,
            DateTime businessDate,
            CustomerPortfolioOptions options)
        {
            if (firstPurchaseDate.HasValue &&
                firstPurchaseDate.Value.Date >= businessDate.Date.AddDays(-options.NewCustomerDaysThreshold))
            {
                return false;
            }

            var category = forecast?.Category ?? CustomerRiskForecastPolicy.CategoryHealthy;
            if (CustomerPortfolioOptimizationPolicy.CompareCategory(category, CustomerRiskForecastPolicy.CategoryWatch) > 0)
                return false;

            if (priorMonthOmzet <= 0)
                return false;

            return mtdOmzet >= priorMonthOmzet;
        }
    }
}
