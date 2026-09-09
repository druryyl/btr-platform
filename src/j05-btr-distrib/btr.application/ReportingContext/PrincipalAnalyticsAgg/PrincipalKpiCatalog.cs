using System.Collections.Generic;
using System.Linq;
using btr.application.ReportingContext.EntityAnalyticsAgg.Models;

namespace btr.application.ReportingContext.PrincipalAnalyticsAgg
{
    /// <summary>
    /// Working Principal KPI catalog. PCM-002 registers PRN-SALES-001 and GR-001.
    /// Other registry families are added by their writer slices. Permanent catalog sync is PCM-019.
    /// </summary>
    public static class PrincipalKpiCatalog
    {
        public const string SalesOutId = "PRN-SALES-001";

        public const string TargetId = "PRN-TGT-001";

        public const string PurchaseInId = "PRN-PUR-001";

        public const string InventoryValueId = "PRN-INV-001";

        public const string InventoryDaysId = "PRN-INV-002";

        public const string GoodReturnAmountId = "PRN-RET-001";

        public const string BrokenReturnAmountId = "PRN-RET-002";

        public const string TotalReturnAmountId = "PRN-RET-003";

        public const string ReturnsMustNotReduceReplaceOrRedefinePrincipalSalesOut =
            "Returns KPIs must not reduce, replace, or redefine PRN-SALES-001.";

        public const string FutureNetSalesMustBeSeparateIdAndMustNotReplacePrincipalSalesOut =
            "A future Net Sales KPI must be a separate ID and must not replace Principal Sales-Out.";

        public const string PrincipalHealthScoreIsExcluded =
            "Principal Health Score is excluded and is not a registered KPI ID.";

        public const string ReturnWritersMustNotWritePrincipalSalesOut =
            "A slice that writes any PRN-RET-* value must not write, overwrite, or recalculate PRN-SALES-001.";

        public const string PrincipalFinancialAttributionIsExcluded =
            "Principal receivable, collection, and credit attribution are excluded. This catalog does not register those KPI IDs.";

        private static readonly PrincipalKpiCatalogEntry SalesOutEntry = CreateSalesOut();

        private static readonly PrincipalKpiCatalogEntry TargetEntry = CreateTarget();

        private static readonly PrincipalKpiCatalogEntry PurchaseInEntry = CreatePurchaseIn();

        private static readonly PrincipalKpiCatalogEntry InventoryValueEntry = CreateInventoryValue();

        private static readonly PrincipalKpiCatalogEntry InventoryDaysEntry = CreateInventoryDays();

        private static readonly PrincipalKpiCatalogEntry GoodReturnAmountEntry = CreateGoodReturnAmount();

        private static readonly PrincipalKpiCatalogEntry BrokenReturnAmountEntry = CreateBrokenReturnAmount();

        private static readonly PrincipalKpiCatalogEntry TotalReturnAmountEntry = CreateTotalReturnAmount();

        private static readonly IReadOnlyList<PrincipalKpiCatalogEntry> RegisteredEntries =
            new[]
            {
                SalesOutEntry,
                TargetEntry,
                PurchaseInEntry,
                InventoryValueEntry,
                InventoryDaysEntry,
                GoodReturnAmountEntry,
                BrokenReturnAmountEntry,
                TotalReturnAmountEntry
            };

        public static IReadOnlyList<PrincipalKpiCatalogEntry> Entries
        {
            get { return RegisteredEntries; }
        }

        public static IReadOnlyList<string> Statements
        {
            get
            {
                return new[]
                {
                    ReturnsMustNotReduceReplaceOrRedefinePrincipalSalesOut,
                    FutureNetSalesMustBeSeparateIdAndMustNotReplacePrincipalSalesOut,
                    PrincipalHealthScoreIsExcluded,
                    ReturnWritersMustNotWritePrincipalSalesOut,
                    PrincipalFinancialAttributionIsExcluded
                };
            }
        }

        public static bool TryGet(string kpiId, out PrincipalKpiCatalogEntry entry)
        {
            entry = RegisteredEntries.FirstOrDefault(item => item.KpiId == kpiId);
            return entry != null;
        }

        private static PrincipalKpiCatalogEntry CreateSalesOut()
        {
            return new PrincipalKpiCatalogEntry
            {
                KpiId = SalesOutId,
                Name = "Principal Sales-Out",
                Description = "Total Sales-Out (DPP) attributed to a Principal",
                EntityCategory = EntityTypeCode.Supplier,
                EvidenceGrain = "Faktur Item",
                Formula = "SUM(FakturItem.SubTotal - FakturItem.DiscRp)",
                IsAuthoritativePrincipalPerformanceKpi = true,
                IsAuthoritativeRankingKpi = true,
                DeductsReturns = false,
                DeductsClaims = false,
                DeductsInventoryAdjustments = false,
                DefinitionStatements = new[]
                {
                    "PRN-SALES-001 is the authoritative Principal performance KPI and is independent of Returns.",
                    "PRN-SALES-001 is the authoritative ranking KPI for Principal performance.",
                    "PRN-SALES-001 Principal Sales-Out (DPP) = SUM(FakturItem.SubTotal - FakturItem.DiscRp).",
                    "Evidence grain is Faktur Item.",
                    "Include only non-void Fakturs (Faktur.VoidDate = '3000-01-01', matching existing Principal omzet evidence).",
                    "Attribute each line through FakturItem.BrgId to BTR_Brg.SupplierId.",
                    "Do not use FakturItem.Total. That amount includes tax (SubTotal - DiscRp + PpnRp).",
                    "Do not use FakturItem.DppRp as the performance measure. Stored DppRp applies DppProsen and is a tax-base amount, not Sales-Out (DPP).",
                    "Do not use Faktur.GrandTotal.",
                    "Do not allocate header tax, freight, rounding, or other header adjustments to Principals.",
                    "Returns, Claims, and Inventory Adjustments do not reduce Sales-Out.",
                    "Do not deduct Returns, Claims, or Inventory Adjustments.",
                    "Returns are independent KPIs and do not redefine Sales-Out.",
                    "Do not write return amounts into the Sales-Out snapshot.",
                    "A slice that writes PRN-SALES-001 must not write any PRN-RET-* value.",
                    "Blank or unknown SupplierId is excluded from Principal totals and written to the Sales-Out data-quality output. Do not create a synthetic Principal.",
                    "Company sales totals on existing company surfaces remain header GrandTotal. They are not required to equal the sum of PRN-SALES-001.",
                    "Totals are not required to reconcile to Faktur GrandTotal.",
                    "Existing BTRPD_SalesmanPrincipalAchievement.CompletedOmzet remains the existing invoice-line Total execution measure. Do not overwrite it with PRN-SALES-001.",
                    "Displays may show Sales-Out and Returns together. The Sales-Out figure shown must equal the stored PRN-SALES-001 value.",
                    "PRN-RET-004 may read PRN-SALES-001 as a denominator. That read does not authorize an update to PRN-SALES-001.",
                    "No Net Sales, net-of-returns, or sales-after-returns KPI is defined.",
                    "A future Net Sales KPI must be a separate ID and must not replace, rename, or become the authoritative meaning of PRN-SALES-001.",
                    "The measure is Principal Sales-Out (DPP) from Faktur Item.",
                    "Tax and header totals are excluded.",
                    "Item Principal comes from current Item master and is treated as immutable for analytics.",
                    "Historical periods use the best available data and may contain documented limitations.",
                    "Unknown Principal and missing monthly target responsibility are visible exceptions, not silent drops of Principal Sales-Out.",
                    "The user-facing name is Principal Sales-Out, not Supplier Omzet and not Principal Omzet.",
                    "This KPI is not a Principal receivable, collection, or credit measure."
                }
            };
        }

        private static PrincipalKpiCatalogEntry CreateTarget()
        {
            return new PrincipalKpiCatalogEntry
            {
                KpiId = TargetId,
                Name = "Principal Target",
                Description = "Sum of Salesman Principal Targets",
                EntityCategory = EntityTypeCode.Supplier,
                EvidenceGrain = "SalesPersonPrincipalTarget",
                Formula = "SUM(BTR_SalesPersonPrincipalTarget.TargetAmount)",
                IsAuthoritativePrincipalPerformanceKpi = false,
                IsAuthoritativeRankingKpi = false,
                DeductsReturns = false,
                DeductsClaims = false,
                DeductsInventoryAdjustments = false,
                DefinitionStatements = new[]
                {
                    "PRN-TGT-001 Principal Target = SUM(BTR_SalesPersonPrincipalTarget.TargetAmount) for that SupplierId, TargetYear, and TargetMonth.",
                    "Evidence grain is SalesPersonPrincipalTarget.",
                    "Principal Target is derived from Salesman Principal Targets.",
                    "No independently maintained Principal Target exists.",
                    "No standalone Principal Target row is created.",
                    "A Salesman is responsible for a Principal in a month only when a target record exists for that Salesman, Principal, year, and month.",
                    "BTR_SalesPersonSupplier is current eligibility reference only. It is not the historical responsibility source and must not override target-based historical responsibility.",
                    "This KPI writer does not write PRN-SALES-001, PRN-TGT-002, PRN-TGT-003, or any return KPI.",
                    "PRN-TGT-001 is not Principal Sales-Out and is not an achievement KPI.",
                    "The user-facing name is Principal Target."
                }
            };
        }

        private static PrincipalKpiCatalogEntry CreatePurchaseIn()
        {
            return new PrincipalKpiCatalogEntry
            {
                KpiId = PurchaseInId,
                Name = "Purchase-In",
                Description = "Total purchases from the Principal",
                EntityCategory = EntityTypeCode.Supplier,
                EvidenceGrain = "Purchase Detail",
                Formula = "SUM(InvoiceItem.Total)",
                IsAuthoritativePrincipalPerformanceKpi = false,
                IsAuthoritativeRankingKpi = false,
                DeductsReturns = false,
                DeductsClaims = false,
                DeductsInventoryAdjustments = false,
                DefinitionStatements = new[]
                {
                    "PRN-PUR-001 Purchase-In = SUM(InvoiceItem.Total) for non-void purchase invoices attributed by Invoice.SupplierId.",
                    "Evidence grain is Purchase Detail.",
                    "Purchase-In remains independent from Sales-Out.",
                    "Purchase-In is not used as the Principal ranking KPI.",
                    "PRN-PUR-001 is not a Principal performance ranking KPI.",
                    "The value is calculated from Purchase Detail. It is not read from Sales-Out history and is not the Purchasing Management in-memory SalesOutAmount.",
                    "Do not use Invoice.GrandTotal. That header amount remains the existing PU-KPI-001 purchasing measure.",
                    "Existing PU-KPI-001 remains unchanged for its current purchasing use.",
                    "Header invoice discounts, tax, and other header adjustments are not allocated to Principals.",
                    "Attribute each Purchase Detail line through Invoice.SupplierId. Do not attribute Purchase-In through item-master SupplierId.",
                    "Include only non-void purchase invoices (Invoice.VoidDate = '3000-01-01', matching existing purchase evidence).",
                    "InvoiceItem.Total is the existing Purchase Detail line total (SubTotal - DiscRp + PpnRp).",
                    "Purchase returns are not deducted. Purchase Detail is the evidence grain.",
                    "This KPI writer does not write, overwrite, or recalculate PRN-SALES-001.",
                    "This KPI is not composed onto Entity Analytics in the writer slice. Entity Analytics purchase pack composition is a later slice.",
                    "The user-facing name is Purchase-In, not Principal Sales-Out and not Principal Omzet."
                }
            };
        }

        private static PrincipalKpiCatalogEntry CreateInventoryValue()
        {
            return new PrincipalKpiCatalogEntry
            {
                KpiId = InventoryValueId,
                Name = "Inventory Value",
                Description = "Current inventory value for Principal products",
                EntityCategory = EntityTypeCode.Supplier,
                EvidenceGrain = "Inventory Snapshot",
                Formula = "SUM(Hpp * Qty) for Principal products on the Inventory Snapshot",
                IsAuthoritativePrincipalPerformanceKpi = false,
                IsAuthoritativeRankingKpi = false,
                DeductsReturns = false,
                DeductsClaims = false,
                DeductsInventoryAdjustments = false,
                DefinitionStatements = new[]
                {
                    "PRN-INV-001 Inventory Value is current inventory value for Principal products.",
                    "Evidence grain is Inventory Snapshot.",
                    "The value uses the existing Inventory Snapshot valuation: SUM(Hpp * Qty), excluding In-Transit and non-positive quantity.",
                    "Attribute Principal products through item-master SupplierId on Inventory Snapshot evidence.",
                    "Blank or unknown SupplierId is excluded. Do not create a synthetic Principal.",
                    "Inventory KPIs are independent operational indicators.",
                    "Inventory KPIs do not modify Sales-Out performance.",
                    "PRN-INV-001 is not a Principal performance ranking KPI.",
                    "This KPI writer does not write, overwrite, or recalculate PRN-SALES-001.",
                    "This KPI is not composed onto Entity Analytics in the writer slice. Entity Analytics inventory pack composition is a later slice.",
                    "This writer does not change IN01-IN05 views.",
                    "The user-facing name is Inventory Value, not Principal Sales-Out."
                }
            };
        }

        private static PrincipalKpiCatalogEntry CreateInventoryDays()
        {
            return new PrincipalKpiCatalogEntry
            {
                KpiId = InventoryDaysId,
                Name = "Inventory Days",
                Description = "Estimated days of inventory coverage",
                EntityCategory = EntityTypeCode.Supplier,
                EvidenceGrain = "Inventory Snapshot",
                Formula = "Eligible quantity ÷ Total ADC",
                IsAuthoritativePrincipalPerformanceKpi = false,
                IsAuthoritativeRankingKpi = false,
                DeductsReturns = false,
                DeductsClaims = false,
                DeductsInventoryAdjustments = false,
                DefinitionStatements = new[]
                {
                    "PRN-INV-002 Inventory Days is estimated days of inventory coverage for Principal products.",
                    "Evidence grain is Inventory Snapshot.",
                    "It uses the existing inventory coverage measure. No new days-of-cover algorithm is introduced.",
                    "The existing coverage measure is Average Days of Supply: eligible quantity ÷ total ADC.",
                    "Eligible quantity and ADC follow the existing inventory coverage rules: active items with positive quantity, excluding Never Sold and Dead Stock, and ADC from the existing inventory forecast policy.",
                    "Inventory Days is null when total ADC is not positive.",
                    "Inventory KPIs are independent operational indicators.",
                    "Inventory KPIs do not modify Sales-Out performance.",
                    "PRN-INV-002 is not a Principal performance ranking KPI.",
                    "This KPI writer does not write, overwrite, or recalculate PRN-SALES-001.",
                    "This KPI is not composed onto Entity Analytics in the writer slice. Entity Analytics inventory pack composition is a later slice.",
                    "This writer does not change IN01-IN05 views.",
                    "The user-facing name is Inventory Days, not Principal Sales-Out."
                }
            };
        }

        private static PrincipalKpiCatalogEntry CreateGoodReturnAmount()
        {
            return new PrincipalKpiCatalogEntry
            {
                KpiId = GoodReturnAmountId,
                Name = "Good Return Amount",
                Description = "Total Good Return value attributed to a Principal",
                EntityCategory = EntityTypeCode.Supplier,
                EvidenceGrain = "Return Item",
                Formula = "SUM(ReturJualItem.SubTotal - ReturJualItem.DiscRp) where JenisRetur = BAGUS",
                IsAuthoritativePrincipalPerformanceKpi = false,
                IsAuthoritativeRankingKpi = false,
                DeductsReturns = false,
                DeductsClaims = false,
                DeductsInventoryAdjustments = false,
                DefinitionStatements = new[]
                {
                    "PRN-RET-001 Good Return Amount = sum of line return amount where JenisRetur = BAGUS.",
                    "Line return amount = ReturJualItem.SubTotal - ReturJualItem.DiscRp.",
                    "Evidence grain is Return Item.",
                    "Attribute each Return Item through ReturJualItem.BrgId to BTR_Brg.SupplierId.",
                    "JenisRetur values are the existing operational values BAGUS and RUSAK only.",
                    "Exclude PpnRp. Do not use ReturJualItem.Total or ReturJual.GrandTotal.",
                    "Void returns are excluded using the existing void sentinel (ReturJual.VoidDate = '3000-01-01').",
                    "Blank or unknown SupplierId is excluded. Do not create a synthetic Principal.",
                    "Salesman on the return document is not used to reassign Faktur revenue, target, or bonus.",
                    "Returns are independent KPIs.",
                    "Returns never reduce Principal Sales-Out.",
                    "PRN-RET-001 is not the authoritative Principal ranking KPI.",
                    "This KPI writer does not write, overwrite, or recalculate PRN-SALES-001.",
                    "This KPI writer does not write PRN-RET-004.",
                    "The user-facing name is Good Return Amount, not Principal Sales-Out and not Net Sales."
                }
            };
        }

        private static PrincipalKpiCatalogEntry CreateBrokenReturnAmount()
        {
            return new PrincipalKpiCatalogEntry
            {
                KpiId = BrokenReturnAmountId,
                Name = "Broken Return Amount",
                Description = "Total Broken/Damaged Return value attributed to a Principal",
                EntityCategory = EntityTypeCode.Supplier,
                EvidenceGrain = "Return Item",
                Formula = "SUM(ReturJualItem.SubTotal - ReturJualItem.DiscRp) where JenisRetur = RUSAK",
                IsAuthoritativePrincipalPerformanceKpi = false,
                IsAuthoritativeRankingKpi = false,
                DeductsReturns = false,
                DeductsClaims = false,
                DeductsInventoryAdjustments = false,
                DefinitionStatements = new[]
                {
                    "PRN-RET-002 Broken Return Amount = sum of line return amount where JenisRetur = RUSAK.",
                    "Line return amount = ReturJualItem.SubTotal - ReturJualItem.DiscRp.",
                    "Evidence grain is Return Item.",
                    "Attribute each Return Item through ReturJualItem.BrgId to BTR_Brg.SupplierId.",
                    "JenisRetur values are the existing operational values BAGUS and RUSAK only.",
                    "Exclude PpnRp. Do not use ReturJualItem.Total or ReturJual.GrandTotal.",
                    "Void returns are excluded using the existing void sentinel (ReturJual.VoidDate = '3000-01-01').",
                    "Blank or unknown SupplierId is excluded. Do not create a synthetic Principal.",
                    "Salesman on the return document is not used to reassign Faktur revenue, target, or bonus.",
                    "Returns are independent KPIs.",
                    "Returns never reduce Principal Sales-Out.",
                    "PRN-RET-002 is not the authoritative Principal ranking KPI.",
                    "This KPI writer does not write, overwrite, or recalculate PRN-SALES-001.",
                    "This KPI writer does not write PRN-RET-004.",
                    "The user-facing name is Broken Return Amount, not Principal Sales-Out and not Net Sales."
                }
            };
        }

        private static PrincipalKpiCatalogEntry CreateTotalReturnAmount()
        {
            return new PrincipalKpiCatalogEntry
            {
                KpiId = TotalReturnAmountId,
                Name = "Total Return Amount",
                Description = "Good Return + Broken Return",
                EntityCategory = EntityTypeCode.Supplier,
                EvidenceGrain = "Return Item",
                Formula = "PRN-RET-001 + PRN-RET-002",
                IsAuthoritativePrincipalPerformanceKpi = false,
                IsAuthoritativeRankingKpi = false,
                DeductsReturns = false,
                DeductsClaims = false,
                DeductsInventoryAdjustments = false,
                DefinitionStatements = new[]
                {
                    "PRN-RET-003 Total Return Amount = PRN-RET-001 + PRN-RET-002.",
                    "Evidence grain is Return Item.",
                    "PRN-RET-003 is not calculated by deducting returns from Principal Sales-Out.",
                    "Returns are independent KPIs.",
                    "Returns never reduce Principal Sales-Out.",
                    "PRN-RET-003 is not the authoritative Principal ranking KPI.",
                    "This KPI writer does not write, overwrite, or recalculate PRN-SALES-001.",
                    "This KPI writer does not write PRN-RET-004.",
                    "The user-facing name is Total Return Amount, not Principal Sales-Out and not Net Sales."
                }
            };
        }
    }

    public sealed class PrincipalKpiCatalogEntry
    {
        public string KpiId { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public string EntityCategory { get; set; }

        public string EvidenceGrain { get; set; }

        public string Formula { get; set; }

        public bool IsAuthoritativePrincipalPerformanceKpi { get; set; }

        public bool IsAuthoritativeRankingKpi { get; set; }

        public bool DeductsReturns { get; set; }

        public bool DeductsClaims { get; set; }

        public bool DeductsInventoryAdjustments { get; set; }

        public IReadOnlyList<string> DefinitionStatements { get; set; }
    }
}
