using btr.application.ReportingContext.DashboardSnapshotAgg.Models;
using btr.application.ReportingContext.DashboardSnapshotAgg.Services;
using FluentAssertions;
using Xunit;

namespace btr.test.ReportingContext
{
    public class CustomerPortfolioTierResolverTest
    {
        private static readonly CustomerPortfolioOptions Options = new CustomerPortfolioOptions();

        [Fact]
        public void Resolve_Strategic_WhenTop10OmzetRank()
        {
            var tier = CustomerPortfolioTierResolver.Resolve(
                omzetRank: 5,
                piutangRank: null,
                mtdOmzet: 1_000_000m,
                openBalance: 0,
                fakturCount6Mo: 1,
                CustomerRiskForecastPolicy.CategoryHealthy,
                Options);

            tier.Should().Be(CustomerPortfolioOptimizationPolicy.TierStrategic);
        }

        [Fact]
        public void Resolve_HighValue_WhenFrequencyAtLeast4()
        {
            var tier = CustomerPortfolioTierResolver.Resolve(
                omzetRank: 50,
                piutangRank: 50,
                mtdOmzet: 100_000m,
                openBalance: 0,
                fakturCount6Mo: 4,
                CustomerRiskForecastPolicy.CategoryHealthy,
                Options);

            tier.Should().Be(CustomerPortfolioOptimizationPolicy.TierHighValue);
        }

        [Fact]
        public void Resolve_LowValue_Default()
        {
            var tier = CustomerPortfolioTierResolver.Resolve(
                omzetRank: null,
                piutangRank: null,
                mtdOmzet: 0,
                openBalance: 0,
                fakturCount6Mo: 0,
                CustomerRiskForecastPolicy.CategoryHealthy,
                Options);

            tier.Should().Be(CustomerPortfolioOptimizationPolicy.TierLowValue);
        }
    }
}
