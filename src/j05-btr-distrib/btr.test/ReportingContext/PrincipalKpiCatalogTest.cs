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
        public void Catalog_RegistersPrincipalTargetAchievementFamily()
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

            PrincipalKpiCatalog.TryGet(PrincipalKpiCatalog.AchievementAmountId, out var amount).Should().BeTrue();
            amount.KpiId.Should().Be("PRN-TGT-002");
            amount.Name.Should().Be("Achievement Amount");
            amount.Description.Should().Be("Principal Sales-Out versus Target");
            amount.EvidenceGrain.Should().Be("PRN-SALES-001 and PRN-TGT-001");
            amount.IsAuthoritativePrincipalPerformanceKpi.Should().BeFalse();
            amount.IsAuthoritativeRankingKpi.Should().BeFalse();
            amount.DeductsReturns.Should().BeFalse();
            amount.DeductsClaims.Should().BeFalse();
            amount.DeductsInventoryAdjustments.Should().BeFalse();
            amount.DefinitionStatements.Should().Contain(
                "PRN-TGT-002 Achievement Amount presents stored PRN-SALES-001 versus stored PRN-TGT-001.");
            amount.DefinitionStatements.Should().Contain(
                "PRN-TGT-002 is not a copy of Sales-Out.");
            amount.DefinitionStatements.Should().Contain(
                "The writer does not write, overwrite, or recalculate PRN-SALES-001.");
            amount.DefinitionStatements.Should().Contain(
                "The writer does not write, overwrite, or recalculate PRN-TGT-001.");
            amount.DefinitionStatements.Should().Contain(
                "Achievement is not labeled Net Sales.");
            amount.Name.Should().NotBe("Net Sales");

            PrincipalKpiCatalog.TryGet(PrincipalKpiCatalog.AchievementPercentageId, out var percentage).Should().BeTrue();
            percentage.KpiId.Should().Be("PRN-TGT-003");
            percentage.Name.Should().Be("Achievement Percentage");
            percentage.Description.Should().Be("Principal Sales-Out ÷ Principal Target");
            percentage.EvidenceGrain.Should().Be("PRN-SALES-001 and PRN-TGT-001");
            percentage.Formula.Should().Be("PRN-SALES-001 ÷ PRN-TGT-001 when PRN-TGT-001 > 0; otherwise null");
            percentage.IsAuthoritativePrincipalPerformanceKpi.Should().BeFalse();
            percentage.IsAuthoritativeRankingKpi.Should().BeFalse();
            percentage.DeductsReturns.Should().BeFalse();
            percentage.DefinitionStatements.Should().Contain(
                "PRN-TGT-003 is null when PRN-TGT-001 is not greater than zero.");
            percentage.DefinitionStatements.Should().Contain(
                "The writer does not write, overwrite, or recalculate PRN-SALES-001.");
            percentage.DefinitionStatements.Should().Contain(
                "The writer does not write, overwrite, or recalculate PRN-TGT-001.");
            percentage.DefinitionStatements.Should().Contain(
                "Achievement is not labeled Net Sales.");
            percentage.Name.Should().NotBe("Net Sales");
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
        public void Catalog_RegistersReturnAmountKpisOnly_AndDoesNotRegisterReturnPercentage()
        {
            PrincipalKpiCatalog.TryGet(PrincipalKpiCatalog.GoodReturnAmountId, out var good).Should().BeTrue();
            good.KpiId.Should().Be("PRN-RET-001");
            good.Name.Should().Be("Good Return Amount");
            good.Description.Should().Be("Total Good Return value attributed to a Principal");
            good.EvidenceGrain.Should().Be("Return Item");
            good.Formula.Should().Contain("BAGUS");
            good.IsAuthoritativePrincipalPerformanceKpi.Should().BeFalse();
            good.IsAuthoritativeRankingKpi.Should().BeFalse();
            good.DefinitionStatements.Should().Contain(
                "PRN-RET-001 Good Return Amount = sum of line return amount where JenisRetur = BAGUS.");
            good.DefinitionStatements.Should().Contain(
                "This KPI writer does not write, overwrite, or recalculate PRN-SALES-001.");
            good.DefinitionStatements.Should().Contain(
                "This KPI writer does not write PRN-RET-004.");

            PrincipalKpiCatalog.TryGet(PrincipalKpiCatalog.BrokenReturnAmountId, out var broken).Should().BeTrue();
            broken.KpiId.Should().Be("PRN-RET-002");
            broken.Name.Should().Be("Broken Return Amount");
            broken.Description.Should().Be("Total Broken/Damaged Return value attributed to a Principal");
            broken.EvidenceGrain.Should().Be("Return Item");
            broken.Formula.Should().Contain("RUSAK");
            broken.IsAuthoritativeRankingKpi.Should().BeFalse();
            broken.DefinitionStatements.Should().Contain(
                "PRN-RET-002 Broken Return Amount = sum of line return amount where JenisRetur = RUSAK.");
            broken.DefinitionStatements.Should().Contain(
                "This KPI writer does not write, overwrite, or recalculate PRN-SALES-001.");

            PrincipalKpiCatalog.TryGet(PrincipalKpiCatalog.TotalReturnAmountId, out var total).Should().BeTrue();
            total.KpiId.Should().Be("PRN-RET-003");
            total.Name.Should().Be("Total Return Amount");
            total.Description.Should().Be("Good Return + Broken Return");
            total.EvidenceGrain.Should().Be("Return Item");
            total.Formula.Should().Be("PRN-RET-001 + PRN-RET-002");
            total.IsAuthoritativeRankingKpi.Should().BeFalse();
            total.DefinitionStatements.Should().Contain(
                "PRN-RET-003 Total Return Amount = PRN-RET-001 + PRN-RET-002.");
            total.DefinitionStatements.Should().Contain(
                "This KPI writer does not write PRN-RET-004.");
        }

        [Fact]
        public void Catalog_RegistersReturnPercentage_AsQualityRatioNotNetSales()
        {
            PrincipalKpiCatalog.TryGet(PrincipalKpiCatalog.ReturnPercentageId, out var percentage).Should().BeTrue();
            percentage.KpiId.Should().Be("PRN-RET-004");
            percentage.Name.Should().Be("Return Percentage");
            percentage.Description.Should().Be("Return Amount ÷ Sales-Out");
            percentage.EvidenceGrain.Should().Be("Return Item and PRN-SALES-001");
            percentage.Formula.Should().Be("PRN-RET-003 ÷ PRN-SALES-001 when PRN-SALES-001 > 0; otherwise null");
            percentage.IsAuthoritativePrincipalPerformanceKpi.Should().BeFalse();
            percentage.IsAuthoritativeRankingKpi.Should().BeFalse();
            percentage.DeductsReturns.Should().BeFalse();
            percentage.DefinitionStatements.Should().Contain(
                "PRN-RET-004 Return Percentage = PRN-RET-003 ÷ PRN-SALES-001 when stored PRN-SALES-001 is greater than zero; otherwise null.");
            percentage.DefinitionStatements.Should().Contain(
                "The writer reads stored PRN-SALES-001 and stored PRN-RET-003.");
            percentage.DefinitionStatements.Should().Contain(
                "The writer does not write, overwrite, or recalculate PRN-SALES-001.");
            percentage.DefinitionStatements.Should().Contain(
                "The writer does not write, overwrite, or recalculate PRN-RET-003.");
            percentage.DefinitionStatements.Should().Contain(
                "Return Percentage is not a deduction from Sales-Out.");
            percentage.DefinitionStatements.Should().Contain(
                "Return Percentage is not Net Sales.");
            percentage.DefinitionStatements.Should().Contain(
                "The user-facing name is Return Percentage, not a deduction from Sales-Out and not Net Sales.");
            percentage.Name.Should().NotBe("Net Sales");
            percentage.Name.Should().NotContain("deduction");
        }

        [Fact]
        public void Catalog_RegistersMomGrowth_AsSalesOutDerivedSupportingKpi()
        {
            PrincipalKpiCatalog.TryGet(PrincipalKpiCatalog.MomGrowthId, out var growth).Should().BeTrue();
            growth.KpiId.Should().Be("PRN-GRW-001");
            growth.Name.Should().Be("Month-over-Month Growth Percentage");
            growth.Description.Should().Be("Monthly Principal growth");
            growth.EvidenceGrain.Should().Be("PRN-SALES-001");
            growth.Formula.Should().Be("(current month PRN-SALES-001 − prior month PRN-SALES-001) ÷ prior month PRN-SALES-001 when prior month > 0; otherwise null");
            growth.IsAuthoritativePrincipalPerformanceKpi.Should().BeFalse();
            growth.IsAuthoritativeRankingKpi.Should().BeFalse();
            growth.DeductsReturns.Should().BeFalse();
            growth.DeductsClaims.Should().BeFalse();
            growth.DeductsInventoryAdjustments.Should().BeFalse();
            growth.DefinitionStatements.Should().Contain(
                "The calculation does not use Purchase-In, returns, claims, or inventory adjustments.");
            growth.DefinitionStatements.Should().Contain(
                "The writer does not write, overwrite, or recalculate PRN-SALES-001.");
            growth.DefinitionStatements.Should().Contain(
                "The writer does not write PRN-GRW-002.");
            growth.DefinitionStatements.Should().Contain(
                "PRN-GRW-001 is not Net Sales.");
            growth.Name.Should().NotBe("Net Sales");

        }

        [Fact]
        public void Catalog_RegistersYoyGrowth_AsSalesOutDerivedSupportingKpi()
        {
            PrincipalKpiCatalog.TryGet(PrincipalKpiCatalog.YoyGrowthId, out var growth).Should().BeTrue();
            growth.KpiId.Should().Be("PRN-GRW-002");
            growth.Name.Should().Be("Year-over-Year Growth Percentage");
            growth.Description.Should().Be("Year-over-year Principal growth");
            growth.EvidenceGrain.Should().Be("PRN-SALES-001");
            growth.Formula.Should().Be("(current month PRN-SALES-001 − same month prior year PRN-SALES-001) ÷ same month prior year PRN-SALES-001 when prior-year month > 0; otherwise null");
            growth.IsAuthoritativePrincipalPerformanceKpi.Should().BeFalse();
            growth.IsAuthoritativeRankingKpi.Should().BeFalse();
            growth.DeductsReturns.Should().BeFalse();
            growth.DeductsClaims.Should().BeFalse();
            growth.DeductsInventoryAdjustments.Should().BeFalse();
            growth.DefinitionStatements.Should().Contain(
                "The calculation does not use Purchase-In, returns, claims, or inventory adjustments.");
            growth.DefinitionStatements.Should().Contain(
                "The writer does not write, overwrite, or recalculate PRN-SALES-001.");
            growth.DefinitionStatements.Should().Contain(
                "The writer does not write PRN-GRW-001.");
            growth.DefinitionStatements.Should().Contain(
                "PRN-GRW-002 is not Net Sales.");
            growth.Name.Should().NotBe("Net Sales");
        }

        [Fact]
        public void Catalog_RegistersActiveCustomerCount_FromProjectionOnly()
        {
            PrincipalKpiCatalog.TryGet(PrincipalKpiCatalog.ActiveCustomerCountId, out var entry).Should().BeTrue();
            entry.KpiId.Should().Be("PRN-CUS-001");
            entry.Name.Should().Be("Active Customer Count");
            entry.Description.Should().Be("Number of active Customers purchasing the Principal");
            entry.EvidenceGrain.Should().Be("Customer × Principal Relationship Projection");
            entry.Formula.Should().Be("COUNT(Customers on BTRPD_CustomerPrincipalRelationship where RelationshipStatus = Active)");
            entry.EntityCategory.Should().Be(EntityTypeCode.Supplier);
            entry.IsAuthoritativePrincipalPerformanceKpi.Should().BeFalse();
            entry.IsAuthoritativeRankingKpi.Should().BeFalse();
            entry.DeductsReturns.Should().BeFalse();
            entry.DeductsClaims.Should().BeFalse();
            entry.DeductsInventoryAdjustments.Should().BeFalse();
            entry.DefinitionStatements.Should().Contain(
                "PRN-CUS-001 Active Customer Count counts Customers on BTRPD_CustomerPrincipalRelationship whose stored status is Active.");
            entry.DefinitionStatements.Should().Contain(
                "The count does not scan raw transaction history.");
            entry.DefinitionStatements.Should().Contain(
                "A Dormant projection row is excluded from the count and is not deleted.");
            entry.DefinitionStatements.Should().Contain(
                "This KPI writer does not write projection status.");
            entry.DefinitionStatements.Should().Contain(
                "This KPI writer does not write, overwrite, or recalculate PRN-SALES-001.");
            entry.DefinitionStatements.Should().Contain(
                "This slice does not render a dashboard panel. Display is a later slice.");
            entry.Name.Should().NotBe("Net Sales");

            PrincipalKpiCatalog.TryGet("PRN-CUS-002", out _).Should().BeTrue();
        }

        [Fact]
        public void Catalog_RegistersCustomerCoverage_FromProjectionAndActiveCustomerCount()
        {
            PrincipalKpiCatalog.TryGet(PrincipalKpiCatalog.CustomerCoverageId, out var entry).Should().BeTrue();
            entry.KpiId.Should().Be("PRN-CUS-002");
            entry.Name.Should().Be("Customer Coverage Percentage");
            entry.Description.Should().Be("Customer reach against the eligible customer base on the projection");
            entry.EvidenceGrain.Should().Be("Customer × Principal Relationship Projection");
            entry.Formula.Should().Be("PRN-CUS-001 ÷ COUNT(Customers on BTRPD_CustomerPrincipalRelationship) when that count is greater than zero; otherwise null");
            entry.EntityCategory.Should().Be(EntityTypeCode.Supplier);
            entry.IsAuthoritativePrincipalPerformanceKpi.Should().BeFalse();
            entry.IsAuthoritativeRankingKpi.Should().BeFalse();
            entry.DeductsReturns.Should().BeFalse();
            entry.DeductsClaims.Should().BeFalse();
            entry.DeductsInventoryAdjustments.Should().BeFalse();
            entry.DefinitionStatements.Should().Contain(
                "PRN-CUS-002 Customer Coverage Percentage = PRN-CUS-001 ÷ count of Customers on that Principal's relationship projection when that count is greater than zero; otherwise null.");
            entry.DefinitionStatements.Should().Contain(
                "The denominator is the retained projection population, including Dormant Customers.");
            entry.DefinitionStatements.Should().Contain(
                "The calculation reads stored PRN-CUS-001 and the stored projection.");
            entry.DefinitionStatements.Should().Contain(
                "The calculation does not scan raw transaction history.");
            entry.DefinitionStatements.Should().Contain(
                "No CP-KPI-* ID is created.");
            entry.DefinitionStatements.Should().Contain(
                "This slice does not render a dashboard panel. Display is a later slice.");
            entry.Name.Should().NotBe("Net Sales");
        }

        [Fact]
        public void Catalog_DoesNotRegisterOtherPrincipalFamiliesOrWithdrawnIds()
        {
            var ids = PrincipalKpiCatalog.Entries.Select(entry => entry.KpiId).ToList();
            var names = PrincipalKpiCatalog.Entries.Select(entry => entry.Name).ToList();

            ids.Should().Contain("PRN-SALES-001");
            ids.Should().Contain("PRN-TGT-001");
            ids.Should().Contain("PRN-TGT-002");
            ids.Should().Contain("PRN-TGT-003");
            ids.Should().Contain("PRN-PUR-001");
            ids.Should().Contain("PRN-INV-001");
            ids.Should().Contain("PRN-INV-002");
            ids.Should().Contain("PRN-RET-001");
            ids.Should().Contain("PRN-RET-002");
            ids.Should().Contain("PRN-RET-003");
            ids.Should().Contain("PRN-RET-004");
            ids.Should().Contain("PRN-GRW-001");
            ids.Should().Contain("PRN-GRW-002");
            ids.Should().Contain("PRN-CUS-001");
            ids.Should().Contain("PRN-CUS-002");
            ids.Should().OnlyContain(id =>
                id == "PRN-SALES-001" ||
                id == "PRN-TGT-001" ||
                id == "PRN-TGT-002" ||
                id == "PRN-TGT-003" ||
                id == "PRN-PUR-001" ||
                id == "PRN-INV-001" ||
                id == "PRN-INV-002" ||
                id == "PRN-RET-001" ||
                id == "PRN-RET-002" ||
                id == "PRN-RET-003" ||
                id == "PRN-RET-004" ||
                id == "PRN-GRW-001" ||
                id == "PRN-GRW-002" ||
                id == "PRN-CUS-001" ||
                id == "PRN-CUS-002");
            ids.Should().NotContain(id => id.StartsWith("PRN-RET-") && id != "PRN-RET-001" && id != "PRN-RET-002" && id != "PRN-RET-003" && id != "PRN-RET-004");
            ids.Should().NotContain(id => id.StartsWith("PRN-TGT-") && id != "PRN-TGT-001" && id != "PRN-TGT-002" && id != "PRN-TGT-003");
            ids.Should().NotContain(id => id.StartsWith("PRN-PUR-") && id != "PRN-PUR-001");
            ids.Should().NotContain(id => id.StartsWith("PRN-INV-") && id != "PRN-INV-001" && id != "PRN-INV-002");
            ids.Should().NotContain(id => id.StartsWith("PRN-CUS-") && id != "PRN-CUS-001" && id != "PRN-CUS-002");
            ids.Should().NotContain(id => id.StartsWith("PRN-GRW-") && id != "PRN-GRW-001" && id != "PRN-GRW-002");
            ids.Should().NotContain(id => id.StartsWith("PR-KPI-"));
            ids.Should().NotContain(id => id.StartsWith("CP-KPI-"));
            ids.Should().NotContain("Net Sales");
            ids.Should().NotContain("Principal Health Score");
            names.Should().NotContain("Net Sales");
            names.Should().NotContain("Principal Health Score");
        }
    }
}
