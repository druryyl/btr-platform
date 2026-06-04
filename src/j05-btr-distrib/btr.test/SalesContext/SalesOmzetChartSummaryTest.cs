using System;
using System.Collections.Generic;
using System.Linq;
using btr.application.SalesContext.OrderFeature;
using btr.application.SalesContext.SalesOmzetAgg.Policies;
using btr.application.SalesContext.SalesOmzetAgg.Services;
using btr.domain.SalesContext.SalesOmzetAgg;
using btr.nuna.Domain;
using FluentAssertions;
using Xunit;

namespace btr.test.SalesContext
{
    public class SalesOmzetChartSummaryTest
    {
        private readonly SalesOmzetChartAmountPolicy _policy = new SalesOmzetChartAmountPolicy();
        private readonly SalesOmzetChartSummaryBuilder _builder;

        public SalesOmzetChartSummaryTest()
        {
            _builder = new SalesOmzetChartSummaryBuilder(_policy);
        }

        private static Periode Jan2025() => new Periode(new DateTime(2025, 1, 1), new DateTime(2025, 1, 31));

        [Fact]
        public void AmountPolicy_Completed_UsesFakturTotal_InRecognized()
        {
            var row = new SalesOmzetView
            {
                OmzetStatus = SalesOmzetStatusEnum.Completed,
                FakturTotal = 1_000_000m,
                OrderTotal = 500_000m
            };

            _policy.ResolveAmount(row).Should().Be(1_000_000m);
            _policy.IncludeInRecognizedTotal(row).Should().BeTrue();
            _policy.IncludeInPipelineTotal(row).Should().BeFalse();
        }

        [Fact]
        public void AmountPolicy_Outstanding_UsesOrderTotal_InPipelineOnly()
        {
            var row = new SalesOmzetView
            {
                OmzetStatus = SalesOmzetStatusEnum.Outstanding,
                FakturTotal = 1_000_000m,
                OrderTotal = 750_000m
            };

            _policy.ResolveAmount(row).Should().Be(750_000m);
            _policy.IncludeInRecognizedTotal(row).Should().BeFalse();
            _policy.IncludeInPipelineTotal(row).Should().BeTrue();
        }

        [Fact]
        public void AmountPolicy_Pending_UsesFakturTotal_InPipelineNotRecognized()
        {
            var row = new SalesOmzetView
            {
                OmzetStatus = SalesOmzetStatusEnum.PendingOmzet,
                FakturTotal = 2_000_000m,
                OrderTotal = 900_000m
            };

            _policy.ResolveAmount(row).Should().Be(2_000_000m);
            _policy.IncludeInRecognizedTotal(row).Should().BeFalse();
            _policy.IncludeInPipelineTotal(row).Should().BeTrue();
        }

        [Fact]
        public void Builder_SumsRecognizedAndPipeline_FromMixedRows()
        {
            var rows = new List<SalesOmzetView>
            {
                new SalesOmzetView
                {
                    OmzetStatus = SalesOmzetStatusEnum.Completed,
                    SaleKind = SaleKindEnum.OrderedSale,
                    OrderId = "O1",
                    FakturTotal = 100m
                },
                new SalesOmzetView
                {
                    OmzetStatus = SalesOmzetStatusEnum.PendingOmzet,
                    FakturTotal = 50m
                },
                new SalesOmzetView
                {
                    OmzetStatus = SalesOmzetStatusEnum.Outstanding,
                    OrderTotal = 30m
                }
            };

            var summary = _builder.Build(rows, Jan2025(), SalesOmzetPeriodFilterMode.SalesPeriod);

            summary.RecognizedOmzet.Should().Be(100m);
            summary.PipelineOmzet.Should().Be(80m);
            summary.RecognizedTransactionCount.Should().Be(1);
            summary.ByStatus.Should().HaveCount(3);
        }

        [Fact]
        public void Builder_EmptyList_ReturnsZeroSummary()
        {
            var summary = _builder.Build(new List<SalesOmzetView>(), Jan2025(), SalesOmzetPeriodFilterMode.OmzetPeriod);

            summary.RecognizedOmzet.Should().Be(0);
            summary.PipelineOmzet.Should().Be(0);
            summary.RecognizedTransactionCount.Should().Be(0);
            summary.ByStatus.Should().BeEmpty();
            summary.ByWeek.Should().HaveCount(5);
            summary.ByWeek.Should().OnlyContain(w => w.RecognizedAmount == 0m);
        }

        [Fact]
        public void Builder_DirectSale_Completed_SeparateSlice()
        {
            var rows = new List<SalesOmzetView>
            {
                new SalesOmzetView
                {
                    OmzetStatus = SalesOmzetStatusEnum.Completed,
                    SaleKind = SaleKindEnum.DirectSale,
                    OrderId = "",
                    FakturTotal = 200m
                }
            };

            var summary = _builder.Build(rows, Jan2025(), SalesOmzetPeriodFilterMode.OmzetPeriod);

            summary.ByStatus.Should().ContainSingle(s =>
                s.Label == SalesOmzetChartSummaryBuilder.LabelDirectSale && s.Amount == 200m);
        }

        [Fact]
        public void WeekGrouper_FullMonth_HasFiveBuckets()
        {
            var buckets = SalesOmzetChartWeekGrouper.BuildBuckets(Jan2025());

            buckets.Should().HaveCount(5);
            buckets[0].WeekStart.Should().Be(new DateTime(2025, 1, 1));
            buckets[4].WeekEnd.Should().Be(new DateTime(2025, 1, 31));
        }

        [Fact]
        public void WeekGrouper_PartialMonth_OnlyIncludesIntersectingWeeks()
        {
            var partial = new Periode(new DateTime(2025, 1, 1), new DateTime(2025, 1, 15));
            var buckets = SalesOmzetChartWeekGrouper.BuildBuckets(partial);

            buckets.Should().HaveCount(3);
            buckets[2].WeekEnd.Should().Be(new DateTime(2025, 1, 15));
        }

        [Fact]
        public void Builder_ByWeek_OmzetPeriod_SumsToRecognizedOmzet()
        {
            var rows = new List<SalesOmzetView>
            {
                new SalesOmzetView
                {
                    OmzetStatus = SalesOmzetStatusEnum.Completed,
                    OmzetDate = new DateTime(2025, 1, 3),
                    FakturTotal = 40m
                },
                new SalesOmzetView
                {
                    OmzetStatus = SalesOmzetStatusEnum.Completed,
                    OmzetDate = new DateTime(2025, 1, 10),
                    FakturTotal = 60m
                },
                new SalesOmzetView
                {
                    OmzetStatus = SalesOmzetStatusEnum.PendingOmzet,
                    OmzetDate = new DateTime(2025, 1, 5),
                    FakturTotal = 999m
                }
            };

            var summary = _builder.Build(rows, Jan2025(), SalesOmzetPeriodFilterMode.OmzetPeriod);

            summary.RecognizedOmzet.Should().Be(100m);
            summary.ByWeek.Sum(w => w.RecognizedAmount).Should().Be(100m);
            summary.ByWeek.First(w => w.WeekStart == new DateTime(2025, 1, 1)).RecognizedAmount.Should().Be(40m);
            summary.ByWeek.First(w => w.WeekStart == new DateTime(2025, 1, 8)).RecognizedAmount.Should().Be(60m);
        }

        [Fact]
        public void ManagerComparison_Top15_ByRecognizedOmzet_Descending()
        {
            var rows = new List<SalesOmzetView>();
            for (var i = 1; i <= 20; i++)
            {
                rows.Add(new SalesOmzetView
                {
                    SalesPersonName = $"Sales-{i:D2}",
                    OmzetStatus = SalesOmzetStatusEnum.Completed,
                    FakturTotal = i * 10m
                });
            }

            rows.Add(new SalesOmzetView
            {
                SalesPersonName = "Sales-05",
                OmzetStatus = SalesOmzetStatusEnum.PendingOmzet,
                FakturTotal = 9999m
            });

            var slices = _builder.BuildManagerComparison(rows);

            slices.Should().HaveCount(15);
            slices[0].SalesPersonName.Should().Be("Sales-20");
            slices[0].RecognizedOmzet.Should().Be(200m);
            slices[14].SalesPersonName.Should().Be("Sales-06");
            slices.Should().OnlyContain(s => s.RecognizedOmzet > 0);
        }

        [Fact]
        public void ManagerComparison_GroupsBySalesPersonName()
        {
            var rows = new List<SalesOmzetView>
            {
                new SalesOmzetView
                {
                    SalesPersonName = "Budi",
                    OmzetStatus = SalesOmzetStatusEnum.Completed,
                    FakturTotal = 100m
                },
                new SalesOmzetView
                {
                    SalesPersonName = "Budi",
                    OmzetStatus = SalesOmzetStatusEnum.Completed,
                    FakturTotal = 50m
                }
            };

            var slices = _builder.BuildManagerComparison(rows);

            slices.Should().ContainSingle(s =>
                s.SalesPersonName == "Budi" && s.RecognizedOmzet == 150m);
        }

        [Fact]
        public void Builder_ByWeek_SalesPeriod_UsesSalesDate_NotOmzetDate()
        {
            var rows = new List<SalesOmzetView>
            {
                new SalesOmzetView
                {
                    OmzetStatus = SalesOmzetStatusEnum.Completed,
                    SalesDate = new DateTime(2025, 1, 5),
                    OmzetDate = new DateTime(2025, 2, 10),
                    FakturTotal = 100m
                }
            };

            var summary = _builder.Build(rows, Jan2025(), SalesOmzetPeriodFilterMode.SalesPeriod);

            summary.ByWeek.First(w => w.WeekStart == new DateTime(2025, 1, 1)).RecognizedAmount.Should().Be(100m);
            summary.ByWeek.First(w => w.WeekStart == new DateTime(2025, 1, 8)).RecognizedAmount.Should().Be(0m);
        }
    }
}
