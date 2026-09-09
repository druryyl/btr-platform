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
        public void Catalog_RegistersPrincipalSalesOut()
        {
            PrincipalKpiCatalog.TryGet("PRN-SALES-001", out var entry).Should().BeTrue();
            entry.Should().NotBeNull();
            entry.KpiId.Should().Be(PrincipalKpiCatalog.SalesOutId);
        }

        [Fact]
        public void PrincipalSalesOut_IsAuthoritativePerformanceAndRankingKpi()
        {
            PrincipalKpiCatalog.TryGet(PrincipalKpiCatalog.SalesOutId, out var entry).Should().BeTrue();

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
            PrincipalKpiCatalog.TryGet(PrincipalKpiCatalog.SalesOutId, out var entry).Should().BeTrue();

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

            PrincipalKpiCatalog.TryGet(PrincipalKpiCatalog.SalesOutId, out var entry).Should().BeTrue();
            entry.DefinitionStatements.Should().Contain(
                "No Net Sales, net-of-returns, or sales-after-returns KPI is defined.");
            entry.DefinitionStatements.Should().Contain(
                "A future Net Sales KPI must be a separate ID and must not replace, rename, or become the authoritative meaning of PRN-SALES-001.");
        }

        [Fact]
        public void Catalog_RegistersPrincipalTargetOnlyForThisFamily()
        {
            PrincipalKpiCatalog.TryGet(PrincipalKpiCatalog.TargetId, out var entry).Should().BeTrue();
            entry.KpiId.Should().Be("PRN-TGT-001");
            entry.Name.Should().Be("Principal Target");
            entry.Description.Should().Be("Sum of Salesman Principal Targets");
            entry.EvidenceGrain.Should().Be("SalesPersonPrincipalTarget");
            entry.Formula.Should().Be("SUM(BTR_SalesPersonPrincipalTarget.TargetAmount)");
            entry.EntityCategory.Should().Be(EntityTypeCode.Supplier);
            entry.IsAuthoritativePrincipalPerformanceKpi.Should().BeFalse();
            entry.IsAuthoritativeRankingKpi.Should().BeFalse();
            entry.DeductsReturns.Should().BeFalse();
            entry.DeductsClaims.Should().BeFalse();
            entry.DeductsInventoryAdjustments.Should().BeFalse();
            entry.DefinitionStatements.Should().Contain(
                "No independently maintained Principal Target exists.");
            entry.DefinitionStatements.Should().Contain(
                "No standalone Principal Target row is created.");
            entry.DefinitionStatements.Should().Contain(
                "BTR_SalesPersonSupplier is current eligibility reference only. It is not the historical responsibility source and must not override target-based historical responsibility.");
            entry.DefinitionStatements.Should().Contain(
                "This KPI writer does not write PRN-SALES-001, PRN-TGT-002, PRN-TGT-003, or any return KPI.");

            PrincipalKpiCatalog.TryGet("PRN-TGT-002", out _).Should().BeFalse();
            PrincipalKpiCatalog.TryGet("PRN-TGT-003", out _).Should().BeFalse();
        }

        [Fact]
        public void Catalog_DoesNotRegisterOtherPrincipalFamiliesOrWithdrawnIds()
        {
            var ids = PrincipalKpiCatalog.Entries.Select(entry => entry.KpiId).ToList();
            var names = PrincipalKpiCatalog.Entries.Select(entry => entry.Name).ToList();

            ids.Should().Contain("PRN-SALES-001");
            ids.Should().Contain("PRN-TGT-001");
            ids.Should().OnlyContain(id => id == "PRN-SALES-001" || id == "PRN-TGT-001");
            ids.Should().NotContain(id => id.StartsWith("PRN-RET-"));
            ids.Should().NotContain("PRN-TGT-002");
            ids.Should().NotContain("PRN-TGT-003");
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
