using System;
using System.Collections.Generic;
using System.Linq;
using btr.application.ReportingContext.DashboardSnapshotAgg.Models;
using btr.application.ReportingContext.DashboardSnapshotAgg.Services;
using FluentAssertions;
using Xunit;

namespace btr.test.ReportingContext
{
    public class CustomerRiskSignalBuilderTest
    {
        private static readonly CustomerRiskForecastOptions DefaultOptions =
            CustomerRiskForecastOptions.FromDashboardOptions(null);

        [Fact]
        public void Build_LikelyLatePayer_Lag10Days()
        {
            var context = BaseContext();
            context.OpenBalance = 5_000_000m;
            context.DueWithinHorizon = 2_000_000m;
            context.AvgPaymentLagDays = 10m;

            var rows = CustomerRiskSignalBuilder.Build(
                context,
                DefaultOptions,
                10_000_000m,
                new DateTime(2026, 6, 15),
                new HashSet<string> { "Days1To30" },
                new HashSet<string>());

            rows.Should().Contain(r => r.SignalKey == CustomerRiskSignalBuilder.SignalLikelyLatePayer);
        }

        [Fact]
        public void Build_ApproachingDormant_65Days()
        {
            var context = BaseContext();
            context.IsActiveThisMonth = false;
            context.DaysSinceLastFaktur = 65;

            var rows = CustomerRiskSignalBuilder.Build(
                context,
                DefaultOptions,
                0m,
                new DateTime(2026, 6, 15),
                new HashSet<string>(),
                new HashSet<string>());

            rows.Should().Contain(r => r.SignalKey == CustomerRiskSignalBuilder.SignalApproachingDormant);
        }

        [Fact]
        public void Build_ProjectedPlafondBreach()
        {
            var context = BaseContext();
            context.Plafond = 10_000_000m;
            context.ProjectedOpenBalance = 12_000_000m;

            var rows = CustomerRiskSignalBuilder.Build(
                context,
                DefaultOptions,
                0m,
                new DateTime(2026, 6, 15),
                new HashSet<string>(),
                new HashSet<string>());

            rows.Should().Contain(r => r.SignalKey == CustomerRiskSignalBuilder.SignalProjectedPlafondBreach);
        }

        [Fact]
        public void Build_DueConcentration20Percent()
        {
            var context = BaseContext();
            context.DueWithinHorizon = 2_000_000m;

            var rows = CustomerRiskSignalBuilder.Build(
                context,
                DefaultOptions,
                10_000_000m,
                new DateTime(2026, 6, 15),
                new HashSet<string>(),
                new HashSet<string>());

            rows.Should().Contain(r => r.SignalKey == CustomerRiskSignalBuilder.SignalDueExposureConcentration);
        }

        [Fact]
        public void Build_DedupeSameSignal_OneRowPerKey()
        {
            var context = BaseContext();
            context.OpenBalance = 5_000_000m;
            context.DueWithinHorizon = 2_000_000m;
            context.AvgPaymentLagDays = 10m;
            context.MinDaysUntilDue = 10;

            var rows = CustomerRiskSignalBuilder.Build(
                context,
                DefaultOptions,
                10_000_000m,
                new DateTime(2026, 6, 15),
                new HashSet<string>(),
                new HashSet<string>());

            Enumerable.Count(rows, r => r.SignalKey == CustomerRiskSignalBuilder.SignalLikelyLatePayer)
                .Should().Be(1);
        }

        [Fact]
        public void LowRecoveryExplanation_UsesInvoiceAttribution_NotAssignedOwner()
        {
            CustomerRiskSignalBuilder.LowRecoveryCustomerExplanation
                .Should().Contain("Last invoicing Salesman");
            CustomerRiskSignalBuilder.LowRecoveryCustomerExplanation
                .Should().Contain("invoice attribution");
            CustomerRiskSignalBuilder.LowRecoveryCustomerExplanation
                .Should().Contain("not Customer ownership");
            CustomerRiskSignalBuilder.LowRecoveryCustomerExplanation
                .Should().NotContain("Assigned salesman");

            var context = BaseContext();
            context.SalesPersonId = "SP1";
            context.OverdueBalance = 1_000_000m;

            var rows = CustomerRiskSignalBuilder.Build(
                context,
                DefaultOptions,
                0m,
                new DateTime(2026, 6, 15),
                new HashSet<string>(),
                new HashSet<string> { "SP1" });

            var row = rows.Single(r => r.SignalKey == CustomerRiskSignalBuilder.SignalLowRecoveryCustomer);
            row.Explanation.Should().Be(CustomerRiskSignalBuilder.LowRecoveryCustomerExplanation);
            row.SignalLabel.Should().Be("Low Recovery Customer");
        }

        private static CustomerRiskForecastContext BaseContext() =>
            new CustomerRiskForecastContext
            {
                CustomerKey = "C001",
                CustomerCode = "C001",
                CustomerName = "Test Customer"
            };
    }
}
