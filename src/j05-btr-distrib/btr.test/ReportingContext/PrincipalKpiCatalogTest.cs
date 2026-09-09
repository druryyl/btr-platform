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
        public void Catalog_RegistersPurchaseInOnlyForThisFamily()
        {
            PrincipalKpiCatalog.TryGet(PrincipalKpiCatalog.PurchaseInId, out var entry).Should().BeTrue();
            entry.KpiId.Should().Be("PRN-PUR-001");
            entry.Name.Should().Be("Purchase-In");
            entry.Description.Should().Be("Total purchases from the Principal");
            entry.EvidenceGrain.Should().Be("Purchase Detail");
            entry.Formula.Should().Be("SUM(InvoiceItem.Total)");
            entry.EntityCategory.Should().Be(EntityTypeCode.Supplier);
            entry.IsAuthoritativePrincipalPerformanceKpi.Should().BeFalse();
            entry.IsAuthoritativeRankingKpi.Should().BeFalse();
            entry.DeductsReturns.Should().BeFalse();
            entry.DeductsClaims.Should().BeFalse();
            entry.DeductsInventoryAdjustments.Should().BeFalse();
            entry.DefinitionStatements.Should().Contain(
                "Purchase-In remains independent from Sales-Out.");
            entry.DefinitionStatements.Should().Contain(
                "Purchase-In is not used as the Principal ranking KPI.");
            entry.DefinitionStatements.Should().Contain(
                "The value is calculated from Purchase Detail. It is not read from Sales-Out history and is not the Purchasing Management in-memory SalesOutAmount.");
            entry.DefinitionStatements.Should().Contain(
                "Existing PU-KPI-001 remains unchanged for its current purchasing use.");
            entry.DefinitionStatements.Should().Contain(
                "This KPI writer does not write, overwrite, or recalculate PRN-SALES-001.");
            entry.DefinitionStatements.Should().Contain(
                "This KPI is not composed onto Entity Analytics in the writer slice. Entity Analytics purchase pack composition is a later slice.");

            PrincipalKpiCatalog.TryGet("PRN-PUR-002", out _).Should().BeFalse();
        }

        [Fact]
        public void Catalog_RegistersInventoryValueAndInventoryDaysOnlyForThisFamily()
        {
            PrincipalKpiCatalog.TryGet(PrincipalKpiCatalog.InventoryValueId, out var value).Should().BeTrue();
            value.KpiId.Should().Be("PRN-INV-001");
            value.Name.Should().Be("Inventory Value");
            value.Description.Should().Be("Current inventory value for Principal products");
            value.EvidenceGrain.Should().Be("Inventory Snapshot");
            value.IsAuthoritativePrincipalPerformanceKpi.Should().BeFalse();
            value.IsAuthoritativeRankingKpi.Should().BeFalse();
            value.DefinitionStatements.Should().Contain(
                "PRN-INV-001 Inventory Value is current inventory value for Principal products.");
            value.DefinitionStatements.Should().Contain(
                "This KPI writer does not write, overwrite, or recalculate PRN-SALES-001.");
            value.DefinitionStatements.Should().Contain(
                "This writer does not change IN01-IN05 views.");

            PrincipalKpiCatalog.TryGet(PrincipalKpiCatalog.InventoryDaysId, out var days).Should().BeTrue();
            days.KpiId.Should().Be("PRN-INV-002");
            days.Name.Should().Be("Inventory Days");
            days.Description.Should().Be("Estimated days of inventory coverage");
            days.EvidenceGrain.Should().Be("Inventory Snapshot");
            days.Formula.Should().Be("Eligible quantity ÷ Total ADC");
            days.IsAuthoritativePrincipalPerformanceKpi.Should().BeFalse();
            days.IsAuthoritativeRankingKpi.Should().BeFalse();
            days.DefinitionStatements.Should().Contain(
                "It uses the existing inventory coverage measure. No new days-of-cover algorithm is introduced.");
            days.DefinitionStatements.Should().Contain(
                "The existing coverage measure is Average Days of Supply: eligible quantity ÷ total ADC.");
            days.DefinitionStatements.Should().Contain(
                "This KPI writer does not write, overwrite, or recalculate PRN-SALES-001.");

            PrincipalKpiCatalog.TryGet("PRN-INV-003", out _).Should().BeFalse();
        }

        [Fact]
        public void Catalog_DoesNotRegisterOtherPrincipalFamiliesOrWithdrawnIds()
        {
            var ids = PrincipalKpiCatalog.Entries.Select(entry => entry.KpiId).ToList();
            var names = PrincipalKpiCatalog.Entries.Select(entry => entry.Name).ToList();

            ids.Should().Contain("PRN-SALES-001");
            ids.Should().Contain("PRN-TGT-001");
            ids.Should().Contain("PRN-PUR-001");
            ids.Should().Contain("PRN-INV-001");
            ids.Should().Contain("PRN-INV-002");
            ids.Should().OnlyContain(id =>
                id == "PRN-SALES-001" ||
                id == "PRN-TGT-001" ||
                id == "PRN-PUR-001" ||
                id == "PRN-INV-001" ||
                id == "PRN-INV-002");
            ids.Should().NotContain(id => id.StartsWith("PRN-RET-"));
            ids.Should().NotContain("PRN-TGT-002");
            ids.Should().NotContain("PRN-TGT-003");
            ids.Should().NotContain(id => id.StartsWith("PRN-PUR-") && id != "PRN-PUR-001");
            ids.Should().NotContain(id => id.StartsWith("PRN-INV-") && id != "PRN-INV-001" && id != "PRN-INV-002");
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
