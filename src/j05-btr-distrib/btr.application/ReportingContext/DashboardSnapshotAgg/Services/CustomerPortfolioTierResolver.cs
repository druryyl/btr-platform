using btr.application.ReportingContext.DashboardSnapshotAgg.Models;

namespace btr.application.ReportingContext.DashboardSnapshotAgg.Services
{
    public static class CustomerPortfolioTierResolver
    {
        public static string Resolve(
            int? omzetRank,
            int? piutangRank,
            decimal mtdOmzet,
            decimal openBalance,
            int fakturCount6Mo,
            string m29Category,
            CustomerPortfolioOptions options)
        {
            options = options ?? CustomerPortfolioOptions.FromDashboardOptions(null);

            if ((omzetRank.HasValue && omzetRank.Value <= CustomerPortfolioOptimizationPolicy.StrategicOmzetRankMax) ||
                (piutangRank.HasValue && piutangRank.Value <= CustomerPortfolioOptimizationPolicy.StrategicPiutangRankMax) ||
                (CustomerPortfolioOptimizationPolicy.IsAtOrAboveCategory(m29Category, CustomerRiskForecastPolicy.CategoryAttention) &&
                 openBalance >= options.StrategicOpenBalanceFloorIdr))
            {
                return CustomerPortfolioOptimizationPolicy.TierStrategic;
            }

            if ((omzetRank.HasValue && omzetRank.Value <= CustomerPortfolioOptimizationPolicy.HighValueOmzetRankMax) ||
                (piutangRank.HasValue && piutangRank.Value <= CustomerPortfolioOptimizationPolicy.HighValuePiutangRankMax) ||
                mtdOmzet >= options.HighValueMtdOmzetFloorIdr ||
                fakturCount6Mo >= options.HighFrequencyFakturCountMin)
            {
                return CustomerPortfolioOptimizationPolicy.TierHighValue;
            }

            if (mtdOmzet > 0 || openBalance > 1 || fakturCount6Mo >= 2)
                return CustomerPortfolioOptimizationPolicy.TierMediumValue;

            return CustomerPortfolioOptimizationPolicy.TierLowValue;
        }
    }
}
