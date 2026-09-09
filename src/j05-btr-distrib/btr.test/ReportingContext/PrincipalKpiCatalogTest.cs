using System.Linq;
using btr.application.ReportingContext.EntityAnalyticsAgg.Models;
using btr.application.ReportingContext.PrincipalAnalyticsAgg;
using FluentAssertions;
using Xunit;

namespace btr.test.ReportingContext
{
    public class PrincipalKpiCatalogTest
    {
        [Fact]
        public void Catalog_RegistersPrincipalSalesOutOnly()
        {
            PrincipalKpiCatalog.Entries.Should().ContainSingle();
            PrincipalKpiCatalog.Entries.Single().KpiId.Should().Be(PrincipalKpiCatalog.SalesOutId);
            PrincipalKpiCatalog.TryGet("PRN-SALES-001", out var entry).Should().BeTrue();
            entry.Should().NotBeNull();
        }

        [Fact]
        public void PrincipalSalesOut_IsAuthoritativePerformanceAndRankingKpi()
        {
            var entry = PrincipalKpiCatalog.Entries.Single();

            entry.KpiId.Should().Be("PRN-SALES-001");
            entry.Name.Should().Be("Principal Sales-Out");
            entry.IsAuthoritativePrincipalPerformanceKpi.Should().BeTrue();
            entry.IsAuthoritativeRankingKpi.Should().BeTrue();
            entry.DefinitionStatements.Should().Contain(
                "PRN-SALES-001 is the authoritative Principal performance KPI and is independent of Returns.");
            entry.DefinitionStatements.Should().Contain(
                "PRN-SALES-001 is the authoritative ranking KPI for Principal performance.");
        }

        [Fact]
        public void PrincipalSalesOut_MatchesPd002AndGr001()
        {
            var entry = PrincipalKpiCatalog.Entries.Single();

            entry.EvidenceGrain.Should().Be("Faktur Item");
            entry.Formula.Should().Be("SUM(FakturItem.SubTotal - FakturItem.DiscRp)");
            entry.EntityCategory.Should().Be(EntityTypeCode.Supplier);
            entry.DeductsReturns.Should().BeFalse();
            entry.DeductsClaims.Should().BeFalse();
            entry.DeductsInventoryAdjustments.Should().BeFalse();

            entry.DefinitionStatements.Should().Contain(
                "Returns, Claims, and Inventory Adjustments do not reduce Sales-Out.");
            entry.DefinitionStatements.Should().Contain(
                "Do not deduct Returns, Claims, or Inventory Adjustments.");
            entry.DefinitionStatements.Should().Contain(
                "Do not use FakturItem.Total. That amount includes tax (SubTotal - DiscRp + PpnRp).");
            entry.DefinitionStatements.Should().Contain(
                "Do not use FakturItem.DppRp as the performance measure. Stored DppRp applies DppProsen and is a tax-base amount, not Sales-Out (DPP).");
            entry.DefinitionStatements.Should().Contain("Do not use Faktur.GrandTotal.");
            entry.DefinitionStatements.Should().Contain(
                "Include only non-void Fakturs (Faktur.VoidDate = '3000-01-01', matching existing Principal omzet evidence).");
            entry.DefinitionStatements.Should().Contain(
                "A slice that writes PRN-SALES-001 must not write any PRN-RET-* value.");
            entry.DefinitionStatements.Should().Contain(
                "Existing BTRPD_SalesmanPrincipalAchievement.CompletedOmzet remains the existing invoice-line Total execution measure. Do not overwrite it with PRN-SALES-001.");
            entry.DefinitionStatements.Should().Contain(
                "Company sales totals on existing company surfaces remain header GrandTotal. They are not required to equal the sum of PRN-SALES-001.");
        }

        [Fact]
        public void Catalog_StatesReturnSemanticProtectionAndNetSalesSeparation()
        {
            PrincipalKpiCatalog.Statements.Should().Contain(
                PrincipalKpiCatalog.ReturnsMustNotReduceReplaceOrRedefinePrincipalSalesOut);
            PrincipalKpiCatalog.Statements.Should().Contain(
                PrincipalKpiCatalog.FutureNetSalesMustBeSeparateIdAndMustNotReplacePrincipalSalesOut);
            PrincipalKpiCatalog.Statements.Should().Contain(
                PrincipalKpiCatalog.ReturnWritersMustNotWritePrincipalSalesOut);
            PrincipalKpiCatalog.Statements.Should().Contain(
                PrincipalKpiCatalog.PrincipalHealthScoreIsExcluded);

            var entry = PrincipalKpiCatalog.Entries.Single();
            entry.DefinitionStatements.Should().Contain(
                "No Net Sales, net-of-returns, or sales-after-returns KPI is defined.");
            entry.DefinitionStatements.Should().Contain(
                "A future Net Sales KPI must be a separate ID and must not replace, rename, or become the authoritative meaning of PRN-SALES-001.");
        }

        [Fact]
        public void Catalog_DoesNotRegisterOtherPrincipalFamiliesOrWithdrawnIds()
        {
            var ids = PrincipalKpiCatalog.Entries.Select(entry => entry.KpiId).ToList();
            var names = PrincipalKpiCatalog.Entries.Select(entry => entry.Name).ToList();

            ids.Should().OnlyContain(id => id == "PRN-SALES-001");
            ids.Should().NotContain(id => id.StartsWith("PRN-RET-"));
            ids.Should().NotContain(id => id.StartsWith("PRN-TGT-"));
            ids.Should().NotContain(id => id.StartsWith("PRN-PUR-"));
            ids.Should().NotContain(id => id.StartsWith("PRN-INV-"));
            ids.Should().NotContain(id => id.StartsWith("PRN-CUS-"));
            ids.Should().NotContain(id => id.StartsWith("PRN-GRW-"));
            ids.Should().NotContain(id => id.StartsWith("PR-KPI-"));
            ids.Should().NotContain(id => id.StartsWith("CP-KPI-"));
            ids.Should().NotContain("Net Sales");
            ids.Should().NotContain("Principal Health Score");
            names.Should().NotContain("Net Sales");
            names.Should().NotContain("Principal Health Score");
        }
    }
}
