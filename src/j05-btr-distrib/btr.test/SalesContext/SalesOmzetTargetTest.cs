using System;
using System.Collections.Generic;
using btr.application.SalesContext.OrderFeature;
using btr.application.SalesContext.SalesOmzetAgg.Contracts;
using btr.application.SalesContext.SalesOmzetAgg.Policies;
using btr.application.SalesContext.SalesOmzetAgg.Services;
using btr.application.SalesContext.SalesPersonAgg.Contracts;
using btr.domain.SalesContext.SalesOmzetAgg;
using btr.domain.SalesContext.SalesPersonAgg;
using btr.nuna.Domain;
using FluentAssertions;
using Xunit;

namespace btr.test.SalesContext
{
    public class SalesOmzetTargetTest
    {
        [Fact]
        public void Achievement_ComputesPercent_UncappedForOverAchievement()
        {
            SalesOmzetChartAchievementPolicy.ComputePercent(120m, 100m).Should().Be(120m);
            SalesOmzetChartAchievementPolicy.FormatPercentDisplay(120m).Should().Be("120.0%");
        }

        [Fact]
        public void Achievement_NoTarget_ReturnsNull()
        {
            SalesOmzetChartAchievementPolicy.ComputePercent(100m, null).Should().NotHaveValue();
            SalesOmzetChartAchievementPolicy.FormatPercentDisplay(null).Should().Be("—");
        }

        [Fact]
        public void TargetYearMonth_UsesEndOfPeriode()
        {
            var periode = new Periode(new DateTime(2025, 1, 1), new DateTime(2025, 3, 15));
            var (year, month) = SalesOmzetTargetResolver.ResolveTargetYearMonth(periode);
            year.Should().Be(2025);
            month.Should().Be(3);
        }

        [Fact]
        public void Resolver_SingleRepInData_ReturnsTarget()
        {
            var targetDal = new StubTargetDal { Amount = 50_000_000m };
            var personDal = new StubSalesPersonDal(new[]
            {
                new SalesPersonModel("SP1") { SalesPersonName = "Budi" }
            });
            var resolver = new SalesOmzetTargetResolver(targetDal, personDal);

            var rows = new List<SalesOmzetView>
            {
                new SalesOmzetView { SalesPersonName = "Budi", OmzetStatus = SalesOmzetStatusEnum.Completed }
            };

            var target = resolver.ResolveTarget(rows, "", Jan2025(), null);
            target.Should().Be(50_000_000m);
            targetDal.LastSalesPersonId.Should().Be("SP1");
            targetDal.LastMonth.Should().Be(1);
        }

        [Fact]
        public void Resolver_MultipleRepsWithoutNarrowSearch_ReturnsNull()
        {
            var targetDal = new StubTargetDal { Amount = 1m };
            var personDal = new StubSalesPersonDal(new[]
            {
                new SalesPersonModel("SP1") { SalesPersonName = "Budi" },
                new SalesPersonModel("SP2") { SalesPersonName = "Ani" }
            });
            var resolver = new SalesOmzetTargetResolver(targetDal, personDal);

            var rows = new List<SalesOmzetView>
            {
                new SalesOmzetView { SalesPersonName = "Budi" },
                new SalesOmzetView { SalesPersonName = "Ani" }
            };

            resolver.ResolveTarget(rows, "", Jan2025(), null).Should().NotHaveValue();
        }

        [Fact]
        public void Builder_WithTarget_SetsAchievementPercent()
        {
            var builder = new SalesOmzetChartSummaryBuilder(new SalesOmzetChartAmountPolicy());
            var rows = new List<SalesOmzetView>
            {
                new SalesOmzetView
                {
                    OmzetStatus = SalesOmzetStatusEnum.Completed,
                    OmzetDate = new DateTime(2025, 1, 10),
                    FakturTotal = 25m
                }
            };

            var summary = builder.Build(rows, Jan2025(), SalesOmzetPeriodFilterMode.OmzetPeriod, 100m);

            summary.Target.Should().Be(100m);
            summary.AchievementPercent.Should().Be(25m);
        }

        private static Periode Jan2025() => new Periode(new DateTime(2025, 1, 1), new DateTime(2025, 1, 31));

        private sealed class StubTargetDal : ISalesOmzetTargetDal
        {
            public decimal? Amount { get; set; }
            public decimal MonthSum { get; set; }
            public string LastSalesPersonId { get; private set; }
            public int LastMonth { get; private set; }

            public decimal? GetTargetAmount(string salesPersonId, int year, int month)
            {
                LastSalesPersonId = salesPersonId;
                LastMonth = month;
                return Amount;
            }

            public decimal SumTargetAmountForMonth(int year, int month) => MonthSum;

            public System.Collections.Generic.IReadOnlyDictionary<string, decimal?> ListTargetsForMonth(int year, int month)
                => new System.Collections.Generic.Dictionary<string, decimal?>();
        }

        private sealed class StubSalesPersonDal : ISalesPersonDal
        {
            private readonly List<SalesPersonModel> _items;

            public StubSalesPersonDal(IEnumerable<SalesPersonModel> items)
            {
                _items = new List<SalesPersonModel>(items);
            }

            public IEnumerable<SalesPersonModel> ListData() => _items;

            public void Insert(SalesPersonModel model) => throw new NotSupportedException();

            public void Update(SalesPersonModel model) => throw new NotSupportedException();

            public void Delete(ISalesPersonKey key) => throw new NotSupportedException();

            public SalesPersonModel GetData(ISalesPersonKey key) => throw new NotSupportedException();
        }
    }
}
