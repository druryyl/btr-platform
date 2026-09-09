using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using btr.application.ReportingContext.DashboardSalesForecastAgg;
using btr.application.ReportingContext.DashboardSalesForecastAgg.Contracts;
using btr.application.ReportingContext.DashboardSalesForecastAgg.Queries;
using btr.application.ReportingContext.DashboardSnapshotAgg.Services;
using btr.application.ReportingContext.PrincipalAnalyticsAgg;
using btr.application.ReportingContext.PrincipalAnalyticsAgg.Contracts;
using btr.application.ReportingContext.PrincipalAnalyticsAgg.Models;
using FluentAssertions;
using Xunit;

namespace btr.test.ReportingContext
{
    public class SalesForecastPrincipalPresentationTest
    {
        private static readonly DateTime BusinessDate = new DateTime(2026, 9, 10);

        [Fact]
        public void Compose_AppliesExistingForecastMethodToSalesOutHistoryAndTarget()
        {
            var history = History(
                HistoryRow(2026, 8, "SUPA", "Alpha", 50m),
                HistoryRow(2026, 9, "SUPB", "Beta", 200m),
                HistoryRow(2026, 9, "SUPA", "Alpha", 900m));
            var target = Target(
                TargetRow("SUPB", "Beta", 800m),
                TargetRow("SUPA", "Alpha", 1000m));

            var presentation = SalesForecastPrincipalPresentationComposer.Compose(
                2026,
                9,
                BusinessDate,
                history,
                target);

            var alphaExpected = SalesForecastPolicy.Compute(
                900m,
                1000m,
                BusinessDate,
                new DateTime(2026, 9, 1),
                new DateTime(2026, 9, 30));
            var betaExpected = SalesForecastPolicy.Compute(
                200m,
                800m,
                BusinessDate,
                new DateTime(2026, 9, 1),
                new DateTime(2026, 9, 30));

            presentation.IsAvailable.Should().BeTrue();
            presentation.SalesOutKpiId.Should().Be(PrincipalKpiCatalog.SalesOutId);
            presentation.TargetKpiId.Should().Be(PrincipalKpiCatalog.TargetId);
            presentation.Items.Select(item => item.SupplierId).Should().Equal("SUPA", "SUPB");
            presentation.Items.Select(item => item.PrincipalSalesOutAmount).Should().Equal(900m, 200m);
            presentation.Items.Should().OnlyContain(item => item.SalesOutKpiId == PrincipalKpiCatalog.SalesOutId);
            presentation.Items.Should().OnlyContain(item => item.TargetKpiId == PrincipalKpiCatalog.TargetId);

            var alpha = presentation.Items.Single(item => item.SupplierId == "SUPA");
            alpha.PrincipalTargetAmount.Should().Be(1000m);
            alpha.ForecastAmount.Should().Be(alphaExpected.ForecastSales);
            alpha.DailyAverageSales.Should().Be(alphaExpected.DailyAverageSales);
            alpha.RequiredDailySales.Should().Be(alphaExpected.RequiredDailySales);
            alpha.TargetGap.Should().Be(alphaExpected.TargetGap);
            alpha.ForecastAchievementPercent.Should().Be(alphaExpected.ForecastAchievementPercent);
            alpha.ForecastAmount.Should().NotBe(50m);

            var beta = presentation.Items.Single(item => item.SupplierId == "SUPB");
            beta.ForecastAmount.Should().Be(betaExpected.ForecastSales);
            presentation.SumOfPrincipalForecasts.Should().Be(alpha.ForecastAmount + beta.ForecastAmount);
            presentation.CompanyForecastNote.Should().Be(
                SalesForecastPrincipalPresentationComposer.SumNotRequiredToEqualCompanyForecastText);
            presentation.Disclosures.Should().Contain(
                SalesForecastPrincipalPresentationComposer.NotRegistryKpiText);
            presentation.Disclosures.Should().Contain(
                SalesForecastPrincipalPresentationComposer.NotNetSalesText);
            presentation.Disclosures.Should().NotContain(statement =>
                statement.IndexOf("Net Sales", StringComparison.OrdinalIgnoreCase) >= 0
                && statement.IndexOf("not Net Sales", StringComparison.OrdinalIgnoreCase) < 0);
        }

        [Fact]
        public void Compose_DoesNotAssignForecastKpiId_OrRankPrincipals()
        {
            var presentation = SalesForecastPrincipalPresentationComposer.Compose(
                2026,
                9,
                BusinessDate,
                History(
                    HistoryRow(2026, 9, "SUPZ", "Zeta", 10m),
                    HistoryRow(2026, 9, "SUPA", "Alpha", 900m)),
                Target(TargetRow("SUPZ", "Zeta", 5000m)));

            presentation.Items.Select(item => item.SupplierId).Should().Equal("SUPA", "SUPZ");
            typeof(DashboardSalesPrincipalForecastPresentation)
                .GetProperties()
                .Select(property => property.Name)
                .Should()
                .NotContain("Rank")
                .And.NotContain("ForecastKpiId");
            typeof(DashboardSalesPrincipalForecastItem)
                .GetProperties()
                .Select(property => property.Name)
                .Should()
                .NotContain("Rank")
                .And.NotContain("ForecastKpiId")
                .And.NotContain("NetSales");
        }

        [Fact]
        public void Compose_IgnoresOtherMonthHistory_AndMismatchedTarget()
        {
            var presentation = SalesForecastPrincipalPresentationComposer.Compose(
                2026,
                9,
                BusinessDate,
                History(HistoryRow(2026, 8, "SUPA", "Alpha", 700m)),
                Target(2026, 8, TargetRow("SUPA", "Alpha", 999m)));

            presentation.IsAvailable.Should().BeFalse();
            presentation.Items.Should().BeEmpty();
            presentation.SumOfPrincipalForecasts.Should().Be(0m);
        }

        [Fact]
        public void Compose_DoesNotUsePurchaseReturnOrHeaderInputs()
        {
            var composerPath = Path.GetFullPath(Path.Combine(
                AppContext.BaseDirectory,
                "..", "..", "..",
                "btr.application",
                "ReportingContext",
                "DashboardSalesForecastAgg",
                "SalesForecastPrincipalPresentationComposer.cs"));
            var source = File.ReadAllText(composerPath);

            source.Should().Contain("SalesForecastPolicy.Compute");
            source.Should().Contain("historyRow?.SalesOutAmount");
            source.Should().Contain("PRN-SALES-001");
            source.Should().Contain("PRN-TGT-001");
            source.Should().Contain("GrandTotal are not inputs");
            source.Should().NotContain("PRN-RET");
            source.Should().NotContain("BTR_Purchase");
            source.Should().NotContain(".GrandTotal");
        }

        [Fact]
        public async Task Handle_KeepsCompanyForecastFigures_AndAddsPrincipalPresentation()
        {
            var handler = new GetDashboardSalesForecastHandler(
                new CompanyForecastDal(),
                new HistoryDal(),
                new TargetDal());

            var response = await handler.Handle(new GetDashboardSalesForecastQuery(), CancellationToken.None);

            response.CurrentSales.Should().Be(5_000_000m);
            response.ForecastSales.Should().Be(8_000_000m);
            response.TotalTarget.Should().Be(9_000_000m);
            response.DailyPace.Should().ContainSingle(row => row.ActualAmount == 5_000_000m);
            response.PrincipalForecast.IsAvailable.Should().BeTrue();
            response.PrincipalForecast.Items.Should().ContainSingle(item =>
                item.SupplierId == "SUPA"
                && item.PrincipalSalesOutAmount == 900m
                && item.PrincipalTargetAmount == 1000m);
            response.ForecastSales.Should().NotBe(response.PrincipalForecast.SumOfPrincipalForecasts);
            response.CurrentSales.Should().NotBe(response.PrincipalForecast.Items.Single().PrincipalSalesOutAmount);
        }

        private static PrincipalSalesOutHistoryResult History(
            params PrincipalSalesOutHistoryRow[] rows)
        {
            return new PrincipalSalesOutHistoryResult
            {
                KpiId = PrincipalKpiCatalog.SalesOutId,
                Months = rows.ToList()
            };
        }

        private static PrincipalSalesOutHistoryRow HistoryRow(
            int year,
            int month,
            string supplierId,
            string name,
            decimal amount)
        {
            return new PrincipalSalesOutHistoryRow
            {
                KpiId = PrincipalKpiCatalog.SalesOutId,
                PeriodYear = year,
                PeriodMonth = month,
                SupplierId = supplierId,
                SupplierName = name,
                SalesOutAmount = amount
            };
        }

        private static PrincipalTargetAggregateResult Target(params PrincipalTargetRow[] rows)
        {
            return Target(2026, 9, rows);
        }

        private static PrincipalTargetAggregateResult Target(
            int year,
            int month,
            params PrincipalTargetRow[] rows)
        {
            return new PrincipalTargetAggregateResult
            {
                KpiId = PrincipalKpiCatalog.TargetId,
                PeriodYear = year,
                PeriodMonth = month,
                Principals = rows.ToList()
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

        private sealed class CompanyForecastDal : IDashboardSalesForecastDal
        {
            public DashboardSalesForecastResponse GetSummary()
            {
                return new DashboardSalesForecastResponse
                {
                    PeriodYear = 2026,
                    PeriodMonth = 9,
                    BusinessDate = BusinessDate,
                    CurrentSales = 5_000_000m,
                    TotalTarget = 9_000_000m,
                    ForecastSales = 8_000_000m,
                    DailyPace = new System.Collections.Generic.List<DashboardSalesDailyPaceItem>
                    {
                        new DashboardSalesDailyPaceItem
                        {
                            PaceDate = BusinessDate,
                            ActualAmount = 5_000_000m
                        }
                    }
                };
            }
        }

        private sealed class HistoryDal : IPrincipalSalesOutHistoryDal
        {
            public PrincipalSalesOutHistoryResult GetHistory()
            {
                return History(HistoryRow(2026, 9, "SUPA", "Alpha", 900m));
            }

            public void ReplaceHistory(PrincipalSalesOutHistoryResult result, string refreshLogId)
            {
            }
        }

        private sealed class TargetDal : IPrincipalTargetSnapshotDal
        {
            public PrincipalTargetAggregateResult GetCurrent()
            {
                return Target(TargetRow("SUPA", "Alpha", 1000m));
            }

            public void ReplaceCurrent(PrincipalTargetAggregateResult result, string refreshLogId)
            {
            }
        }
    }
}
