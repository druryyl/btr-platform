using System;
using System.Linq;
using btr.application.ReportingContext.PrincipalAnalyticsAgg;
using btr.application.ReportingContext.PrincipalAnalyticsAgg.Contracts;
using btr.application.ReportingContext.PrincipalAnalyticsAgg.Models;
using btr.application.ReportingContext.PrincipalAnalyticsAgg.Queries;
using btr.infrastructure.ReportingContext.PrincipalAnalyticsAgg;
using FluentAssertions;
using Xunit;

namespace btr.test.ReportingContext
{
    public class PrincipalPerformanceQueryTest
    {
        [Fact]
        public void Compose_RanksByStoredPrincipalSalesOut_AndCountsUnknownPrincipalExceptions()
        {
            var snapshot = new PrincipalSalesOutAggregateResult
            {
                KpiId = PrincipalKpiCatalog.SalesOutId,
                PeriodYear = 2026,
                PeriodMonth = 9,
                GeneratedAt = new DateTime(2026, 9, 9, 8, 0, 0),
                Principals = new[]
                {
                    Row("SUPB", "Beta", 200m, 2),
                    Row("SUPA", "Alpha", 900m, 1)
                }.ToList(),
                DataQuality = new[]
                {
                    new PrincipalSalesOutDataQualityRow
                    {
                        ExceptionCode = PrincipalSalesOutSnapshot.BlankSupplierExceptionCode,
                        Amount = 50m,
                        LineCount = 2
                    },
                    new PrincipalSalesOutDataQualityRow
                    {
                        ExceptionCode = PrincipalSalesOutSnapshot.UnknownSupplierExceptionCode,
                        Amount = 25m,
                        LineCount = 1
                    }
                }.ToList()
            };

            var response = PrincipalPerformanceComposer.Compose(snapshot);

            response.IsAvailable.Should().BeTrue();
            response.KpiId.Should().Be(PrincipalKpiCatalog.SalesOutId);
            response.KpiName.Should().Be("Principal Sales-Out");
            response.PrincipalSalesOutAmount.Should().Be(1100m);
            response.UnknownPrincipalExceptionCount.Should().Be(3);
            response.Ranking.Select(row => row.SupplierId).Should().Equal("SUPA", "SUPB");
            response.Ranking.Should().OnlyContain(row => row.KpiId == PrincipalKpiCatalog.SalesOutId);
            response.Disclosures.Should().Contain(
                PrincipalSalesOutDisclosure.ReturnsDoNotReduceOrRedefinePrincipalSalesOut);
            response.Disclosures.Should().Contain(
                "The measure is Principal Sales-Out (DPP) from Faktur Item.");
            response.Disclosures.Should().Contain(
                "Unknown Principal and missing monthly target responsibility are visible exceptions, not silent drops of Principal Sales-Out.");
        }

        [Fact]
        public void Compose_WhenSnapshotMissing_DoesNotInventAPrincipalSalesOutFigure()
        {
            var response = PrincipalPerformanceComposer.Compose(null);

            response.IsAvailable.Should().BeFalse();
            response.PrincipalSalesOutAmount.Should().Be(0m);
            response.Ranking.Should().BeEmpty();
            response.KpiId.Should().Be(PrincipalKpiCatalog.SalesOutId);
            response.Disclosures.Should().NotBeEmpty();
        }

        [Fact]
        public void Compose_AttachesStoredTargetAndAchievement_WithoutChangingSalesOut()
        {
            var snapshot = new PrincipalSalesOutAggregateResult
            {
                KpiId = PrincipalKpiCatalog.SalesOutId,
                PeriodYear = 2026,
                PeriodMonth = 9,
                GeneratedAt = new DateTime(2026, 9, 9, 8, 0, 0),
                Principals = new[]
                {
                    Row("SUPB", "Beta", 200m, 2),
                    Row("SUPA", "Alpha", 900m, 1)
                }.ToList()
            };
            var target = new PrincipalTargetAggregateResult
            {
                KpiId = PrincipalKpiCatalog.TargetId,
                PeriodYear = 2026,
                PeriodMonth = 9,
                GeneratedAt = new DateTime(2026, 9, 9, 8, 0, 0),
                Principals = new[]
                {
                    TargetRow("SUPA", "Alpha", 800m),
                    TargetRow("SUPB", "Beta", 50m)
                }.ToList()
            };
            var achievement = new PrincipalAchievementResult
            {
                AchievementAmountKpiId = PrincipalKpiCatalog.AchievementAmountId,
                AchievementPercentageKpiId = PrincipalKpiCatalog.AchievementPercentageId,
                SalesOutKpiId = PrincipalKpiCatalog.SalesOutId,
                TargetKpiId = PrincipalKpiCatalog.TargetId,
                PeriodYear = 2026,
                PeriodMonth = 9,
                GeneratedAt = new DateTime(2026, 9, 9, 8, 0, 0),
                Principals = new[]
                {
                    AchievementRow("SUPA", 900m, 800m, 100m, 1.125000m),
                    AchievementRow("SUPB", 200m, 50m, 150m, 4.000000m)
                }.ToList()
            };

            var response = PrincipalPerformanceComposer.Compose(snapshot, target, achievement);

            response.PrincipalSalesOutAmount.Should().Be(1100m);
            response.Ranking.Select(row => row.SupplierId).Should().Equal("SUPA", "SUPB");
            response.TargetKpiId.Should().Be(PrincipalKpiCatalog.TargetId);
            response.PrincipalTargetAmount.Should().Be(850m);
            response.AchievementAmountKpiId.Should().Be(PrincipalKpiCatalog.AchievementAmountId);
            response.AchievementPercentageKpiId.Should().Be(PrincipalKpiCatalog.AchievementPercentageId);
            response.AchievementAmount.Should().Be(250m);
            response.AchievementPercentage.Should().BeApproximately(1.294118m, 0.000001m);
            response.TargetAchievementIsAvailable.Should().BeTrue();
            response.MissingTargetExceptionCount.Should().Be(0);
            var alpha = response.Ranking.Single(row => row.SupplierId == "SUPA");
            alpha.PrincipalSalesOutAmount.Should().Be(900m);
            alpha.PrincipalTargetAmount.Should().Be(800m);
            alpha.AchievementAmount.Should().Be(100m);
            alpha.AchievementPercentage.Should().Be(1.125000m);
            response.Ranking.Should().OnlyContain(row => row.KpiId == PrincipalKpiCatalog.SalesOutId);
        }

        [Fact]
        public void Compose_KeepsSalesOutVisible_WhenTargetIsMissing()
        {
            var snapshot = new PrincipalSalesOutAggregateResult
            {
                KpiId = PrincipalKpiCatalog.SalesOutId,
                PeriodYear = 2026,
                PeriodMonth = 9,
                GeneratedAt = new DateTime(2026, 9, 9, 8, 0, 0),
                Principals = new[]
                {
                    Row("SUPA", "Alpha", 900m, 1),
                    Row("SUPC", "Charlie", 300m, 2)
                }.ToList()
            };
            var target = new PrincipalTargetAggregateResult
            {
                KpiId = PrincipalKpiCatalog.TargetId,
                PeriodYear = 2026,
                PeriodMonth = 9,
                GeneratedAt = new DateTime(2026, 9, 9, 8, 0, 0),
                Principals = new[] { TargetRow("SUPA", "Alpha", 800m) }.ToList()
            };

            var response = PrincipalPerformanceComposer.Compose(snapshot, target, null);

            response.PrincipalSalesOutAmount.Should().Be(1200m);
            response.Ranking.Should().HaveCount(2);
            var charlie = response.Ranking.Single(row => row.SupplierId == "SUPC");
            charlie.PrincipalSalesOutAmount.Should().Be(300m);
            charlie.PrincipalTargetAmount.Should().BeNull();
            charlie.AchievementAmount.Should().BeNull();
            charlie.AchievementPercentage.Should().BeNull();
            response.MissingTargetExceptionCount.Should().Be(1);
            response.PrincipalTargetAmount.Should().Be(800m);
        }

        [Fact]
        public void Compose_DoesNotAttachTargetFromADifferentPeriod()
        {
            var snapshot = new PrincipalSalesOutAggregateResult
            {
                KpiId = PrincipalKpiCatalog.SalesOutId,
                PeriodYear = 2026,
                PeriodMonth = 9,
                GeneratedAt = new DateTime(2026, 9, 9, 8, 0, 0),
                Principals = new[] { Row("SUPA", "Alpha", 100m, 1) }.ToList()
            };
            var target = new PrincipalTargetAggregateResult
            {
                KpiId = PrincipalKpiCatalog.TargetId,
                PeriodYear = 2026,
                PeriodMonth = 8,
                GeneratedAt = new DateTime(2026, 8, 9, 8, 0, 0),
                Principals = new[] { TargetRow("SUPA", "Alpha", 999m) }.ToList()
            };

            var response = PrincipalPerformanceComposer.Compose(snapshot, target, null);

            response.PrincipalSalesOutAmount.Should().Be(100m);
            response.PrincipalTargetAmount.Should().BeNull();
            response.TargetAchievementIsAvailable.Should().BeFalse();
            response.Ranking.Single().PrincipalTargetAmount.Should().BeNull();
            response.Ranking.Single().PrincipalSalesOutAmount.Should().Be(100m);
        }

        private static PrincipalTargetRow TargetRow(string supplierId, string supplierName, decimal targetAmount)
        {
            return new PrincipalTargetRow
            {
                KpiId = PrincipalKpiCatalog.TargetId,
                SupplierId = supplierId,
                SupplierName = supplierName,
                TargetAmount = targetAmount
            };
        }

        private static PrincipalAchievementRow AchievementRow(
            string supplierId,
            decimal salesOutAmount,
            decimal targetAmount,
            decimal achievementAmount,
            decimal achievementPercentage)
        {
            return new PrincipalAchievementRow
            {
                AchievementAmountKpiId = PrincipalKpiCatalog.AchievementAmountId,
                AchievementPercentageKpiId = PrincipalKpiCatalog.AchievementPercentageId,
                SalesOutKpiId = PrincipalKpiCatalog.SalesOutId,
                TargetKpiId = PrincipalKpiCatalog.TargetId,
                SupplierId = supplierId,
                SupplierName = supplierId,
                SalesOutAmount = salesOutAmount,
                TargetAmount = targetAmount,
                AchievementAmount = achievementAmount,
                AchievementPercentage = achievementPercentage
            };
        }

        [Fact]
        public void EvidenceComposer_UsesFakturItemSalesOut_AndKeepsPrincipalIdentity()
        {
            var response = PrincipalSalesOutEvidenceComposer.Compose(
                "SUPA",
                2026,
                9,
                new[]
                {
                    Line("FK002", "FI002", "SUPA", "Alpha", subTotal: 400m, discRp: 40m, fakturDate: new DateTime(2026, 9, 3)),
                    Line("FK001", "FI001", "SUPA", "Alpha", subTotal: 1000m, discRp: 100m, fakturDate: new DateTime(2026, 9, 2)),
                    Line("FK003", "FI003", "SUPB", "Beta", subTotal: 500m, discRp: 0m, fakturDate: new DateTime(2026, 9, 4))
                });

            response.KpiId.Should().Be(PrincipalKpiCatalog.SalesOutId);
            response.PrincipalName.Should().Be("Alpha");
            response.Lines.Should().HaveCount(2);
            response.Lines.Select(line => line.FakturItemId).Should().Equal("FI001", "FI002");
            response.Lines.Should().OnlyContain(line => line.KpiId == PrincipalKpiCatalog.SalesOutId);
            response.PrincipalSalesOutAmount.Should().Be(1260m);
            response.Lines.Should().OnlyContain(line => line.SupplierId == "SUPA");
        }

        [Fact]
        public void EvidenceSql_ReadsFakturItem_NotPurchasingManagementSalesOutAmount()
        {
            PrincipalSalesOutEvidenceDal.ListFakturItemEvidenceForPrincipalSql.Should().Contain("BTR_FakturItem");
            PrincipalSalesOutEvidenceDal.ListFakturItemEvidenceForPrincipalSql.Should().Contain("fi.SubTotal");
            PrincipalSalesOutEvidenceDal.ListFakturItemEvidenceForPrincipalSql.Should().Contain("fi.DiscRp");
            PrincipalSalesOutEvidenceDal.ListFakturItemEvidenceForPrincipalSql.Should().Contain("f.VoidDate = '3000-01-01'");
            PrincipalSalesOutEvidenceDal.ListFakturItemEvidenceForPrincipalSql.Should().Contain("b.SupplierId");
            PrincipalSalesOutEvidenceDal.ListFakturItemEvidenceForPrincipalSql.Should().NotContain("Retur");
            PrincipalSalesOutEvidenceDal.ListFakturItemEvidenceForPrincipalSql.Should().NotContain("PRN-RET");
            PrincipalSalesOutEvidenceDal.ListFakturItemEvidenceForPrincipalSql.Should().NotContain("SalesOutAmount");
            PrincipalSalesOutEvidenceDal.ListFakturItemEvidenceForPrincipalSql.Should().NotContain("Purchasing");
        }

        [Fact]
        public void Compose_AttachesStoredReturns_WithoutChangingSalesOut()
        {
            var snapshot = new PrincipalSalesOutAggregateResult
            {
                KpiId = PrincipalKpiCatalog.SalesOutId,
                PeriodYear = 2026,
                PeriodMonth = 9,
                GeneratedAt = new DateTime(2026, 9, 9, 8, 0, 0),
                Principals = new[]
                {
                    Row("SUPA", "Alpha", 1000m, 1),
                    Row("SUPB", "Beta", 500m, 2)
                }.ToList()
            };
            var returns = new PrincipalReturnAggregateResult
            {
                GoodReturnKpiId = PrincipalKpiCatalog.GoodReturnAmountId,
                BrokenReturnKpiId = PrincipalKpiCatalog.BrokenReturnAmountId,
                TotalReturnKpiId = PrincipalKpiCatalog.TotalReturnAmountId,
                PeriodYear = 2026,
                PeriodMonth = 9,
                GeneratedAt = new DateTime(2026, 9, 9, 8, 0, 0),
                Principals = new[]
                {
                    ReturnRow("SUPA", "Alpha", 80m, 20m, 100m),
                    ReturnRow("SUPB", "Beta", 10m, 5m, 15m)
                }.ToList()
            };
            var percentages = new PrincipalReturnPercentageResult
            {
                ReturnPercentageKpiId = PrincipalKpiCatalog.ReturnPercentageId,
                SalesOutKpiId = PrincipalKpiCatalog.SalesOutId,
                TotalReturnKpiId = PrincipalKpiCatalog.TotalReturnAmountId,
                PeriodYear = 2026,
                PeriodMonth = 9,
                GeneratedAt = new DateTime(2026, 9, 9, 8, 0, 0),
                Principals = new[]
                {
                    PercentageRow("SUPA", "Alpha", 100m, 1000m, 0.100000m),
                    PercentageRow("SUPB", "Beta", 15m, 500m, 0.030000m)
                }.ToList()
            };

            var response = PrincipalPerformanceComposer.Compose(snapshot, null, null, returns, percentages);

            response.PrincipalSalesOutAmount.Should().Be(1500m);
            response.Ranking.Select(row => row.SupplierId).Should().Equal("SUPA", "SUPB");
            response.GoodReturnAmountKpiId.Should().Be(PrincipalKpiCatalog.GoodReturnAmountId);
            response.BrokenReturnAmountKpiId.Should().Be(PrincipalKpiCatalog.BrokenReturnAmountId);
            response.TotalReturnAmountKpiId.Should().Be(PrincipalKpiCatalog.TotalReturnAmountId);
            response.ReturnPercentageKpiId.Should().Be(PrincipalKpiCatalog.ReturnPercentageId);
            response.GoodReturnAmount.Should().Be(90m);
            response.BrokenReturnAmount.Should().Be(25m);
            response.TotalReturnAmount.Should().Be(115m);
            response.ReturnPercentage.Should().BeApproximately(0.076667m, 0.000001m);
            response.ReturnIsAvailable.Should().BeTrue();
            var alpha = response.Ranking.Single(row => row.SupplierId == "SUPA");
            alpha.PrincipalSalesOutAmount.Should().Be(1000m);
            alpha.GoodReturnAmount.Should().Be(80m);
            alpha.BrokenReturnAmount.Should().Be(20m);
            alpha.TotalReturnAmount.Should().Be(100m);
            alpha.ReturnPercentage.Should().Be(0.100000m);
            alpha.GoodReturnAmountKpiId.Should().Be(PrincipalKpiCatalog.GoodReturnAmountId);
            alpha.ReturnPercentageKpiId.Should().Be(PrincipalKpiCatalog.ReturnPercentageId);
            response.Ranking.Should().OnlyContain(row => row.KpiId == PrincipalKpiCatalog.SalesOutId);
        }

        [Fact]
        public void Compose_DoesNotAttachReturnsFromADifferentPeriod()
        {
            var snapshot = new PrincipalSalesOutAggregateResult
            {
                KpiId = PrincipalKpiCatalog.SalesOutId,
                PeriodYear = 2026,
                PeriodMonth = 9,
                GeneratedAt = new DateTime(2026, 9, 9, 8, 0, 0),
                Principals = new[] { Row("SUPA", "Alpha", 100m, 1) }.ToList()
            };
            var returns = new PrincipalReturnAggregateResult
            {
                GoodReturnKpiId = PrincipalKpiCatalog.GoodReturnAmountId,
                BrokenReturnKpiId = PrincipalKpiCatalog.BrokenReturnAmountId,
                TotalReturnKpiId = PrincipalKpiCatalog.TotalReturnAmountId,
                PeriodYear = 2026,
                PeriodMonth = 8,
                GeneratedAt = new DateTime(2026, 8, 9, 8, 0, 0),
                Principals = new[] { ReturnRow("SUPA", "Alpha", 50m, 50m, 100m) }.ToList()
            };

            var response = PrincipalPerformanceComposer.Compose(snapshot, null, null, returns, null);

            response.PrincipalSalesOutAmount.Should().Be(100m);
            response.ReturnIsAvailable.Should().BeFalse();
            response.GoodReturnAmount.Should().BeNull();
            response.TotalReturnAmount.Should().BeNull();
            response.ReturnPercentage.Should().BeNull();
            response.Ranking.Single().GoodReturnAmount.Should().BeNull();
            response.Ranking.Single().PrincipalSalesOutAmount.Should().Be(100m);
        }

        [Fact]
        public void ReturnEvidenceComposer_UsesReturnItemAmounts_AndKeepsPrincipalIdentity()
        {
            var response = PrincipalReturnEvidenceComposer.Compose(
                "SUPA",
                2026,
                9,
                new[]
                {
                    ReturnLine("R002", "RC002", "L002", "SUPA", "Alpha", "RUSAK", 200m, 20m, new DateTime(2026, 9, 3)),
                    ReturnLine("R001", "RC001", "L001", "SUPA", "Alpha", "BAGUS", 1000m, 100m, new DateTime(2026, 9, 2)),
                    ReturnLine("R003", "RC003", "L003", "SUPB", "Beta", "BAGUS", 500m, 0m, new DateTime(2026, 9, 4))
                });

            response.GoodReturnAmountKpiId.Should().Be(PrincipalKpiCatalog.GoodReturnAmountId);
            response.BrokenReturnAmountKpiId.Should().Be(PrincipalKpiCatalog.BrokenReturnAmountId);
            response.TotalReturnAmountKpiId.Should().Be(PrincipalKpiCatalog.TotalReturnAmountId);
            response.ReturnPercentageKpiId.Should().Be(PrincipalKpiCatalog.ReturnPercentageId);
            response.PrincipalName.Should().Be("Alpha");
            response.Lines.Should().HaveCount(2);
            response.Lines.Select(line => line.ReturJualItemId).Should().Equal("L001", "L002");
            response.GoodReturnAmount.Should().Be(900m);
            response.BrokenReturnAmount.Should().Be(180m);
            response.TotalReturnAmount.Should().Be(1080m);
            response.Lines.Should().OnlyContain(line => line.SupplierId == "SUPA");
        }

        [Fact]
        public void ReturnEvidenceSql_ReadsReturnItem_NotSalesOut()
        {
            PrincipalReturnEvidenceDal.ListReturnItemEvidenceForPrincipalSql.Should().Contain("BTR_ReturJualItem");
            PrincipalReturnEvidenceDal.ListReturnItemEvidenceForPrincipalSql.Should().Contain("BTR_ReturJual");
            PrincipalReturnEvidenceDal.ListReturnItemEvidenceForPrincipalSql.Should().Contain("ri.SubTotal");
            PrincipalReturnEvidenceDal.ListReturnItemEvidenceForPrincipalSql.Should().Contain("ri.DiscRp");
            PrincipalReturnEvidenceDal.ListReturnItemEvidenceForPrincipalSql.Should().Contain("VoidDate = '3000-01-01'");
            PrincipalReturnEvidenceDal.ListReturnItemEvidenceForPrincipalSql.Should().Contain("BAGUS");
            PrincipalReturnEvidenceDal.ListReturnItemEvidenceForPrincipalSql.Should().Contain("RUSAK");
            PrincipalReturnEvidenceDal.ListReturnItemEvidenceForPrincipalSql.Should().Contain("b.SupplierId");
            PrincipalReturnEvidenceDal.ListReturnItemEvidenceForPrincipalSql.Should().NotContain("BTR_Faktur");
            PrincipalReturnEvidenceDal.ListReturnItemEvidenceForPrincipalSql.Should().NotContain("BTRPD_PrincipalSalesOut");
            PrincipalReturnEvidenceDal.ListReturnItemEvidenceForPrincipalSql.Should().NotContain("PRN-SALES-001");
            PrincipalReturnEvidenceDal.ListReturnItemEvidenceForPrincipalSql.Should().NotContain("GrandTotal");
            PrincipalReturnEvidenceDal.ListReturnItemEvidenceForPrincipalSql.Should().NotContain("SalesOutAmount");
        }

        private static PrincipalSalesOutRow Row(
            string supplierId,
            string supplierName,
            decimal salesOutAmount,
            int sortOrder)
        {
            return new PrincipalSalesOutRow
            {
                KpiId = PrincipalKpiCatalog.SalesOutId,
                SupplierId = supplierId,
                SupplierName = supplierName,
                SalesOutAmount = salesOutAmount,
                LineCount = 1,
                SortOrder = sortOrder
            };
        }

        private static PrincipalSalesOutFakturItemEvidenceLine Line(
            string fakturId,
            string fakturItemId,
            string supplierId,
            string supplierName,
            decimal subTotal,
            decimal discRp,
            DateTime fakturDate)
        {
            return new PrincipalSalesOutFakturItemEvidenceLine
            {
                FakturId = fakturId,
                FakturCode = fakturId,
                FakturDate = fakturDate,
                FakturItemId = fakturItemId,
                BrgId = "BRG1",
                ItemSupplierId = supplierId,
                SupplierId = supplierId,
                SupplierName = supplierName,
                SubTotal = subTotal,
                DiscRp = discRp
            };
        }

        private static PrincipalReturnRow ReturnRow(
            string supplierId,
            string supplierName,
            decimal goodReturnAmount,
            decimal brokenReturnAmount,
            decimal totalReturnAmount)
        {
            return new PrincipalReturnRow
            {
                GoodReturnKpiId = PrincipalKpiCatalog.GoodReturnAmountId,
                BrokenReturnKpiId = PrincipalKpiCatalog.BrokenReturnAmountId,
                TotalReturnKpiId = PrincipalKpiCatalog.TotalReturnAmountId,
                SupplierId = supplierId,
                SupplierName = supplierName,
                GoodReturnAmount = goodReturnAmount,
                BrokenReturnAmount = brokenReturnAmount,
                TotalReturnAmount = totalReturnAmount,
                LineCount = 1
            };
        }

        private static PrincipalReturnPercentageRow PercentageRow(
            string supplierId,
            string supplierName,
            decimal totalReturnAmount,
            decimal salesOutAmount,
            decimal? returnPercentage)
        {
            return new PrincipalReturnPercentageRow
            {
                ReturnPercentageKpiId = PrincipalKpiCatalog.ReturnPercentageId,
                SalesOutKpiId = PrincipalKpiCatalog.SalesOutId,
                TotalReturnKpiId = PrincipalKpiCatalog.TotalReturnAmountId,
                SupplierId = supplierId,
                SupplierName = supplierName,
                TotalReturnAmount = totalReturnAmount,
                SalesOutAmount = salesOutAmount,
                ReturnPercentage = returnPercentage
            };
        }

        private static PrincipalReturnItemEvidenceLine ReturnLine(
            string returJualId,
            string returJualCode,
            string returJualItemId,
            string supplierId,
            string supplierName,
            string jenisRetur,
            decimal subTotal,
            decimal discRp,
            DateTime returJualDate)
        {
            return new PrincipalReturnItemEvidenceLine
            {
                ReturJualId = returJualId,
                ReturJualCode = returJualCode,
                ReturJualDate = returJualDate,
                ReturJualItemId = returJualItemId,
                BrgId = "BRG1",
                BrgCode = "BRG1",
                JenisRetur = jenisRetur,
                ItemSupplierId = supplierId,
                SupplierId = supplierId,
                SupplierName = supplierName,
                SubTotal = subTotal,
                DiscRp = discRp
            };
        }
    }
}
