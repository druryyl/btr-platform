using btr.application.SalesContext.SalesOmzetAgg.Contracts;
using btr.application.SalesContext.SalesPersonPrincipalTargetAgg.Contracts;
using btr.domain.SalesContext.SalesPersonPrincipalTargetAgg;
using btr.infrastructure.Helpers;
using btr.infrastructure.SalesContext.SalesOmzetAgg;
using FluentAssertions;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace btr.test.SalesContext
{
    public class SalesOmzetTargetResolutionTest
    {
        [Fact]
        public void GetTargetAmount_PrincipalSumSupersedesLegacy()
        {
            var principalDal = new StubPrincipalTargetDal
            {
                SalesPersonSums = new Dictionary<string, decimal>
                {
                    ["SP1"] = 800_000_000m
                }
            };

            var sut = CreateSut(principalDal);

            sut.GetTargetAmount("SP1", 2026, 1).Should().Be(800_000_000m);
        }

        [Fact]
        public void SumTargetAmountForMonth_PrincipalOnly_NoLegacyRows_DoesNotThrow()
        {
            var principalDal = new StubPrincipalTargetDal
            {
                SalesPersonSums = new Dictionary<string, decimal>
                {
                    ["SP1"] = 700_000m,
                    ["SP2"] = 750_000m
                }
            };

            var sut = CreateSutWithLegacyTable(principalDal);

            sut.SumTargetAmountForMonth(2099, 12).Should().Be(1_450_000m);
        }

        [Fact]
        public void ListTargetsForMonth_PrincipalOnly_NoLegacyRows_DoesNotThrow()
        {
            var principalDal = new StubPrincipalTargetDal
            {
                SalesPersonSums = new Dictionary<string, decimal>
                {
                    ["SP1"] = 700_000m,
                    ["SP2"] = 750_000m
                }
            };

            var sut = CreateSutWithLegacyTable(principalDal);

            var targets = sut.ListTargetsForMonth(2099, 12);

            targets.Should().HaveCount(2);
            targets["SP1"].Should().Be(700_000m);
            targets["SP2"].Should().Be(750_000m);
        }

        private static SalesOmzetTargetDal CreateSut(
            StubPrincipalTargetDal principalDal,
            string dbName = "devTest")
        {
            return new SalesOmzetTargetDal(
                Options.Create(new DatabaseOptions { ServerName = "JUDE7", DbName = dbName, IsTest = true }),
                principalDal);
        }

        private static SalesOmzetTargetDal CreateSutWithLegacyTable(StubPrincipalTargetDal principalDal)
        {
            return CreateSut(principalDal, "btr");
        }

        private sealed class StubPrincipalTargetDal : ISalesPersonPrincipalTargetDal
        {
            public Dictionary<string, decimal> SalesPersonSums { get; set; } =
                new Dictionary<string, decimal>();

            public IEnumerable<SalesPersonPrincipalTargetModel> ListBySalesPersonPeriod(
                string salesPersonId, int year, int month)
                => Enumerable.Empty<SalesPersonPrincipalTargetModel>();

            public IEnumerable<SalesPersonPrincipalTargetModel> ListByPeriod(int year, int month)
                => Enumerable.Empty<SalesPersonPrincipalTargetModel>();

            public void Upsert(IEnumerable<SalesPersonPrincipalTargetModel> rows) { }

            public decimal SumBySalesPersonPeriod(string salesPersonId, int year, int month)
            {
                return SalesPersonSums.TryGetValue(salesPersonId, out var sum) ? sum : 0m;
            }

            public IReadOnlyDictionary<string, decimal> SumByPeriod(int year, int month)
            {
                return SalesPersonSums;
            }
        }
    }
}
