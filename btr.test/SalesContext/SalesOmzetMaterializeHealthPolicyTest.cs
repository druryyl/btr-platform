using System;
using btr.application.SalesContext.SalesOmzetAgg;
using btr.application.SalesContext.SalesOmzetAgg.Policies;
using btr.nuna.Domain;
using FluentAssertions;
using Xunit;

namespace btr.test.SalesContext
{
    public class SalesOmzetMaterializeHealthPolicyTest
    {
        [Fact]
        public void ResolveWindow_Uses60DayBucketEndingOnWindowEnd()
        {
            var windowEnd = new DateTime(2025, 11, 30);

            var window = SalesOmzetMaterializeHealthWindow.Resolve(windowEnd);

            window.Tgl2.Date.Should().Be(windowEnd.Date);
            window.Tgl1.Date.Should().Be(windowEnd.Date.AddDays(-60));
        }

        [Fact]
        public void Evaluate_Good_WhenNoGapsAndFreshReconcileRelativeToWindowEnd()
        {
            var windowEnd = new DateTime(2025, 11, 30);
            var window = new Periode(windowEnd.AddDays(-60), windowEnd);
            var metrics = new SalesOmzetMaterializeHealthMetrics
            {
                MissingOrders = 0,
                MissingDirectFakturs = 0,
                UnlinkedFakturs = 0,
                StaleFakturEstimate = 0,
                LastReconciledMax = windowEnd.AddDays(-3)
            };

            SalesOmzetMaterializeHealthPolicy.Evaluate(metrics, window)
                .Should().Be(SalesOmzetMaterializeHealthLevel.Good);
        }

        [Fact]
        public void Evaluate_Poor_WhenManyMissing()
        {
            var windowEnd = new DateTime(2025, 11, 30);
            var window = new Periode(windowEnd.AddDays(-60), windowEnd);
            var metrics = new SalesOmzetMaterializeHealthMetrics
            {
                MissingOrders = 50,
                MissingDirectFakturs = 0,
                UnlinkedFakturs = 0,
                LastReconciledMax = windowEnd.AddDays(-1)
            };

            SalesOmzetMaterializeHealthPolicy.Evaluate(metrics, window)
                .Should().Be(SalesOmzetMaterializeHealthLevel.Poor);
        }

        [Fact]
        public void Evaluate_Poor_WhenReconcileOlderThan30DaysBeforeWindowEnd_NotToday()
        {
            var windowEnd = new DateTime(2025, 11, 30);
            var window = new Periode(windowEnd.AddDays(-60), windowEnd);
            var metrics = new SalesOmzetMaterializeHealthMetrics
            {
                MissingOrders = 0,
                MissingDirectFakturs = 0,
                UnlinkedFakturs = 0,
                StaleFakturEstimate = 0,
                LastReconciledMax = windowEnd.AddDays(-31)
            };

            SalesOmzetMaterializeHealthPolicy.Evaluate(metrics, window)
                .Should().Be(SalesOmzetMaterializeHealthLevel.Poor);
        }

        [Fact]
        public void Evaluate_Warning_WhenSmallMissingCount()
        {
            var windowEnd = new DateTime(2025, 11, 30);
            var window = new Periode(windowEnd.AddDays(-60), windowEnd);
            var metrics = new SalesOmzetMaterializeHealthMetrics
            {
                MissingOrders = 2,
                MissingDirectFakturs = 0,
                UnlinkedFakturs = 0,
                LastReconciledMax = windowEnd.AddDays(-1)
            };

            SalesOmzetMaterializeHealthPolicy.Evaluate(metrics, window)
                .Should().Be(SalesOmzetMaterializeHealthLevel.Warning);
        }

        [Fact]
        public void Evaluate_Warning_WhenReconcileBetween7And30DaysBeforeWindowEnd()
        {
            var windowEnd = new DateTime(2025, 11, 30);
            var window = new Periode(windowEnd.AddDays(-60), windowEnd);
            var metrics = new SalesOmzetMaterializeHealthMetrics
            {
                MissingOrders = 0,
                MissingDirectFakturs = 0,
                UnlinkedFakturs = 0,
                StaleFakturEstimate = 0,
                LastReconciledMax = windowEnd.AddDays(-15)
            };

            SalesOmzetMaterializeHealthPolicy.Evaluate(metrics, window)
                .Should().Be(SalesOmzetMaterializeHealthLevel.Warning);
        }
    }
}
