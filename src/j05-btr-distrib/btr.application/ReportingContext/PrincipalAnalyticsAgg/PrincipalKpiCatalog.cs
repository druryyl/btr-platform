using System.Collections.Generic;
using System.Linq;
using btr.application.ReportingContext.EntityAnalyticsAgg.Models;

namespace btr.application.ReportingContext.PrincipalAnalyticsAgg
{
    /// <summary>
    /// Working Principal KPI catalog. PCM-002 registers PRN-SALES-001 and GR-001 only.
    /// Other registry families are added by their writer slices. Permanent catalog sync is PCM-019.
    /// </summary>
    public static class PrincipalKpiCatalog
    {
        public const string SalesOutId = "PRN-SALES-001";

        public const string TargetId = "PRN-TGT-001";

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

        private static readonly IReadOnlyList<PrincipalKpiCatalogEntry> RegisteredEntries =
            new[] { SalesOutEntry, TargetEntry };

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
