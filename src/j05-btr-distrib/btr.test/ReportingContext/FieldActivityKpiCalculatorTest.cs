using System;
using System.Collections.Generic;
using btr.application.ReportingContext.DashboardFieldActivityAgg.Contracts;
using btr.application.ReportingContext.DashboardFieldActivityOverviewAgg.Contracts;
using btr.application.ReportingContext.DashboardFieldActivityOverviewAgg.Services;
using btr.domain.SalesContext.VisitPlanAgg;
using FluentAssertions;
using Xunit;

namespace btr.test.ReportingContext
{
    public class FieldActivityKpiCalculatorTest
    {
        [Fact]
        public void Compute_WhenAllPlannedVisited_Execution100()
        {
            var result = FieldActivityKpiCalculator.Compute(
                new[] { Plan("C1", 1), Plan("C2", 2) },
                new[] { CheckIn("C1"), CheckIn("C2") },
                new HashSet<string>(StringComparer.OrdinalIgnoreCase),
                Array.Empty<FieldActivityOrderBatchRow>());

            result.PlannedVisits.Should().Be(2);
            result.ActualVisits.Should().Be(2);
            result.VisitExecutionPercent.Should().Be(100);
            result.MissedVisits.Should().Be(0);
        }

        [Fact]
        public void Compute_WhenOrderOnVisitedCustomer_CountsEffectiveCallAndOrders()
        {
            var orders = new List<FieldActivityOrderBatchRow>
            {
                new FieldActivityOrderBatchRow { CustomerId = "C1", TotalAmount = 100000m }
            };

            var result = FieldActivityKpiCalculator.Compute(
                new[] { Plan("C1", 1) },
                new[] { CheckIn("C1") },
                new HashSet<string>(new[] { "C1" }, StringComparer.OrdinalIgnoreCase),
                orders);

            result.EffectiveCalls.Should().Be(1);
            result.OrdersCount.Should().Be(1);
            result.OmzetAmount.Should().Be(100000m);
        }

        [Fact]
        public void Compute_WhenPlannedZero_VisitExecutionPercentIsNull()
        {
            FieldActivityKpiCalculator.Compute(
                    Array.Empty<EffectiveVisitPlanEntry>(),
                    new[] { CheckIn("C9") },
                    new HashSet<string>(StringComparer.OrdinalIgnoreCase),
                    Array.Empty<FieldActivityOrderBatchRow>())
                .VisitExecutionPercent.Should().BeNull();
        }

        private static EffectiveVisitPlanEntry Plan(string customerId, int noUrut)
        {
            return new EffectiveVisitPlanEntry
            {
                CustomerId = customerId,
                CustomerCode = customerId,
                CustomerName = customerId,
                NoUrut = noUrut
            };
        }

        private static FieldActivityCheckInRow CheckIn(string customerId)
        {
            return new FieldActivityCheckInRow
            {
                CustomerId = customerId,
                CustomerCode = customerId,
                CustomerName = customerId,
                CheckInTime = "08:00:00",
                CheckInLatitude = -6.2,
                CheckInLongitude = 106.8,
                CustomerLatitude = -6.2,
                CustomerLongitude = 106.8,
                Accuracy = 10
            };
        }
    }
}
