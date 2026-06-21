using System.Collections.Generic;
using btr.application.ReportingContext.DashboardSnapshotAgg.Models;
using btr.application.ReportingContext.DashboardSnapshotAgg.Services;
using FluentAssertions;
using Xunit;

namespace btr.test.ReportingContext
{
    public class CustomerRiskRecommendationBuilderTest
    {
        [Fact]
        public void Build_Critical_ManagementReview()
        {
            var context = new CustomerRiskForecastContext
            {
                Category = CustomerRiskForecastPolicy.CategoryCritical
            };

            CustomerRiskRecommendationBuilder.Build(context).RecommendationKey
                .Should().Be(CustomerRiskRecommendationBuilder.ManagementReview);
        }

        [Fact]
        public void Build_Due5dAndSlowPayer_EarlyCollection()
        {
            var context = new CustomerRiskForecastContext
            {
                Category = CustomerRiskForecastPolicy.CategoryAttention,
                MinDaysUntilDue = 5,
                OpenBalance = 5_000_000m,
                Signals = new List<CustomerRiskSignalContext>
                {
                    new CustomerRiskSignalContext
                    {
                        SignalKey = CustomerRiskSignalBuilder.SignalLikelyLatePayer,
                        Severity = CustomerRiskForecastPolicy.SeverityModerate
                    }
                }
            };

            CustomerRiskRecommendationBuilder.Build(context).RecommendationKey
                .Should().Be(CustomerRiskRecommendationBuilder.EarlyCollection);
        }

        [Fact]
        public void Build_SevereDecline_SalesRecovery()
        {
            var context = new CustomerRiskForecastContext
            {
                Category = CustomerRiskForecastPolicy.CategoryAttention,
                Signals = new List<CustomerRiskSignalContext>
                {
                    new CustomerRiskSignalContext
                    {
                        SignalKey = CustomerRiskSignalBuilder.SignalSevereDecline,
                        Severity = CustomerRiskForecastPolicy.SeverityStrong
                    }
                }
            };

            CustomerRiskRecommendationBuilder.Build(context).RecommendationKey
                .Should().Be(CustomerRiskRecommendationBuilder.SalesRecovery);
        }
    }
}
