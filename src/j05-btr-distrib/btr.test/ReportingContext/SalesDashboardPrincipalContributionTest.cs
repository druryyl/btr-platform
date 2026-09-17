using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using btr.application.ReportingContext.DashboardSalesAgg;
using btr.application.ReportingContext.DashboardSalesAgg.Contracts;
using btr.application.ReportingContext.DashboardSalesAgg.Queries;
using btr.application.ReportingContext.PrincipalAnalyticsAgg;
using btr.application.ReportingContext.PrincipalAnalyticsAgg.Contracts;
using btr.application.ReportingContext.PrincipalAnalyticsAgg.Models;
using FluentAssertions;
using Xunit;

namespace btr.test.ReportingContext
{
    public class SalesDashboardPrincipalContributionTest
    {
        [Fact]
        public void Compose_RanksByPrincipalSalesOut_AndAttachesStoredPrincipalTarget()
        {
            var salesOut = new PrincipalSalesOutAggregateResult
            {
                KpiId = PrincipalKpiCatalog.SalesOutId,
                PeriodYear = 2026,
                PeriodMonth = 9,
                Principals = new[]
                {
                    SalesRow("SUPB", "Beta", 200m),
                    SalesRow("SUPA", "Alpha", 900m)
                }.ToList()
            };
            var target = new PrincipalTargetAggregateResult
            {
                KpiId = PrincipalKpiCatalog.TargetId,
                PeriodYear = 2026,
                PeriodMonth = 9,
                Principals = new[]
                {
                    TargetRow("SUPB", "Beta", 800m),
                    TargetRow("SUPA", "Alpha", 100m)
                }.ToList()
            };

            var contribution = SalesDashboardPrincipalContributionComposer.Compose(salesOut, target);

            contribution.IsAvailable.Should().BeTrue();
            contribution.SalesOutKpiId.Should().Be(PrincipalKpiCatalog.SalesOutId);
            contribution.TargetKpiId.Should().Be(PrincipalKpiCatalog.TargetId);
            contribution.PrincipalSalesOutAmount.Should().Be(1100m);
            contribution.PrincipalTargetAmount.Should().Be(900m);
            contribution.Ranking.Select(row => row.SupplierId).Should().Equal("SUPA", "SUPB");
            contribution.Ranking.Select(row => row.PrincipalSalesOutAmount).Should().Equal(900m, 200m);
            contribution.Ranking.Should().OnlyContain(row => row.SalesOutKpiId == PrincipalKpiCatalog.SalesOutId);
            contribution.Ranking.Single(row => row.SupplierId == "SUPA").PrincipalTargetAmount.Should().Be(100m);
            contribution.Ranking.Should().OnlyContain(row => row.TargetKpiId == PrincipalKpiCatalog.TargetId);
            contribution.Disclosures.Should().Contain(
                PrincipalSalesOutDisclosure.ReturnsDoNotReduceOrRedefinePrincipalSalesOut);
            contribution.CompanyHeaderNote.Should().Be(
                SalesDashboardPrincipalContributionComposer.CompanyHeaderNoteText);
        }

        [Fact]
        public void Compose_DoesNotRankByPrincipalTarget_WhenTargetExceedsSalesOut()
        {
            var salesOut = new PrincipalSalesOutAggregateResult
            {
                KpiId = PrincipalKpiCatalog.SalesOutId,
                PeriodYear = 2026,
                PeriodMonth = 9,
                Principals = new[]
                {
                    SalesRow("LOW", "Low Sales", 50m),
                    SalesRow("HIGH", "High Sales", 500m)
                }.ToList()
            };
            var target = new PrincipalTargetAggregateResult
            {
                KpiId = PrincipalKpiCatalog.TargetId,
                PeriodYear = 2026,
                PeriodMonth = 9,
                Principals = new[]
                {
                    TargetRow("LOW", "Low Sales", 5000m),
                    TargetRow("HIGH", "High Sales", 10m)
                }.ToList()
            };

            var contribution = SalesDashboardPrincipalContributionComposer.Compose(salesOut, target);

            contribution.Ranking.Select(row => row.SupplierId).Should().Equal("HIGH", "LOW");
            contribution.Ranking[0].PrincipalTargetAmount.Should().Be(10m);
        }

        [Fact]
        public void Compose_WhenSalesOutMissing_DoesNotInventPrincipalSalesOut()
        {
            var contribution = SalesDashboardPrincipalContributionComposer.Compose(null, null);

            contribution.IsAvailable.Should().BeFalse();
            contribution.PrincipalSalesOutAmount.Should().Be(0m);
            contribution.PrincipalTargetAmount.Should().BeNull();
            contribution.Ranking.Should().BeEmpty();
            contribution.SalesOutKpiId.Should().Be(PrincipalKpiCatalog.SalesOutId);
        }

        [Fact]
        public void Compose_DoesNotAttachTargetFromADifferentPeriod()
        {
            var salesOut = new PrincipalSalesOutAggregateResult
            {
                KpiId = PrincipalKpiCatalog.SalesOutId,
                PeriodYear = 2026,
                PeriodMonth = 9,
                Principals = new[] { SalesRow("SUPA", "Alpha", 100m) }.ToList()
            };
            var target = new PrincipalTargetAggregateResult
            {
                KpiId = PrincipalKpiCatalog.TargetId,
                PeriodYear = 2026,
                PeriodMonth = 8,
                Principals = new[] { TargetRow("SUPA", "Alpha", 999m) }.ToList()
            };

            var contribution = SalesDashboardPrincipalContributionComposer.Compose(salesOut, target);

            contribution.PrincipalTargetAmount.Should().BeNull();
            contribution.Ranking.Single().PrincipalTargetAmount.Should().BeNull();
            contribution.Ranking.Single().PrincipalSalesOutAmount.Should().Be(100m);
        }

        [Fact]
        public async Task Handle_KeepsCompanyHeaderTotals_AndAddsPrincipalContribution()
        {
            var handler = new GetDashboardSalesHandler(
                new CompanySnapshotDal(),
                new SalesOutSnapshotDal(),
                new TargetSnapshotDal());

            var response = await handler.Handle(new GetDashboardSalesQuery(), CancellationToken.None);

            response.TotalOmzet.Should().Be(5_000_000m);
            response.CompletedOmzet.Should().Be(5_000_000m);
            response.TotalAchievement.Should().Be(5_000_000m);
            response.TopSalesmanRanking.Should().ContainSingle(row => row.SalesPersonName == "Alice");
            response.PrincipalContribution.IsAvailable.Should().BeTrue();
            response.PrincipalContribution.PrincipalSalesOutAmount.Should().Be(1100m);
            response.PrincipalContribution.PrincipalTargetAmount.Should().Be(900m);
            response.PrincipalContribution.Ranking.Select(row => row.SupplierId).Should().Equal("SUPA", "SUPB");
            response.TotalOmzet.Should().NotBe(response.PrincipalContribution.PrincipalSalesOutAmount);
        }

        private static PrincipalSalesOutRow SalesRow(string supplierId, string name, decimal amount)
        {
            return new PrincipalSalesOutRow
            {
                KpiId = PrincipalKpiCatalog.SalesOutId,
                SupplierId = supplierId,
                SupplierName = name,
                SalesOutAmount = amount
            };
        }

        private static PrincipalTargetRow TargetRow(string supplierId, string name, decimal amount)
        {
            return new PrincipalTargetRow
            {
                KpiId = PrincipalKpiCatalog.TargetId,
                SupplierId = supplierId,
                SupplierName = name,
                TargetAmount = amount
            };
        }

        private sealed class CompanySnapshotDal : IDashboardSalesDal
        {
            public DashboardSalesResponse GetSummary()
            {
                return new DashboardSalesResponse
                {
                    TotalOmzet = 5_000_000m,
                    CompletedOmzet = 5_000_000m,
                    TotalAchievement = 5_000_000m,
                    TopSalesmanRanking = new System.Collections.Generic.List<DashboardSalesRankingItem>
                    {
                        new DashboardSalesRankingItem
                        {
                            Rank = 1,
                            SalesPersonName = "Alice",
                            CompletedOmzet = 3_000_000m
                        }
                    }
                };
            }
        }

        private sealed class SalesOutSnapshotDal : IPrincipalSalesOutSnapshotDal
        {
            public PrincipalSalesOutAggregateResult GetCurrent()
            {
                return new PrincipalSalesOutAggregateResult
                {
                    KpiId = PrincipalKpiCatalog.SalesOutId,
                    PeriodYear = 2026,
                    PeriodMonth = 9,
                    Principals = new[]
                    {
                        SalesRow("SUPB", "Beta", 200m),
                        SalesRow("SUPA", "Alpha", 900m)
                    }.ToList()
                };
            }

            public void ReplaceCurrent(PrincipalSalesOutAggregateResult result, string refreshLogId)
            {
            }
        }

        private sealed class TargetSnapshotDal : IPrincipalTargetSnapshotDal
        {
            public PrincipalTargetAggregateResult GetCurrent()
            {
                return new PrincipalTargetAggregateResult
                {
                    KpiId = PrincipalKpiCatalog.TargetId,
                    PeriodYear = 2026,
                    PeriodMonth = 9,
                    Principals = new[]
                    {
                        TargetRow("SUPB", "Beta", 800m),
                        TargetRow("SUPA", "Alpha", 100m)
                    }.ToList()
                };
            }

            public void ReplaceCurrent(PrincipalTargetAggregateResult result, string refreshLogId)
            {
            }
        }
    }
}
