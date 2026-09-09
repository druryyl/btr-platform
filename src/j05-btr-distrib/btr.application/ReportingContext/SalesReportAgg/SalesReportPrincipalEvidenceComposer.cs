using System;
using System.Collections.Generic;
using System.Linq;
using btr.application.ReportingContext.PrincipalAnalyticsAgg;
using btr.application.ReportingContext.PrincipalAnalyticsAgg.Contracts;
using btr.application.ReportingContext.PrincipalAnalyticsAgg.Models;
using btr.application.ReportingContext.PrincipalAnalyticsAgg.Queries;
using btr.application.ReportingContext.PrincipalAnalyticsAgg.Services;
using btr.application.ReportingContext.SalesReportAgg.Queries;
using btr.nuna.Domain;

namespace btr.application.ReportingContext.SalesReportAgg
{
    /// <summary>
    /// SA03 Principal evidence. Attributes PRN-SALES-001 from Faktur Item amounts
    /// and Brg.SupplierId. Does not allocate Faktur header totals.
    /// </summary>
    public static class SalesReportPrincipalEvidenceComposer
    {
        public const string KpiName = "Principal Sales-Out";

        public const string HeaderTotalLabel = "Header Total";

        public const string MeasureIsPrnSales001AndNotRequiredToEqualGrandTotal =
            "The measure is PRN-SALES-001 Principal Sales-Out and is not required to equal Faktur GrandTotal.";

        public const string ReturnsClaimsAndInventoryAdjustmentsAreNotDeducted =
            "Returns, claims, and inventory adjustments are not deducted from the Principal amount and do not redefine it.";

        public const string CompanyHeaderTotalsAreNotPrincipalSales =
            "Company report totals that remain header-based are header totals, not Principal sales.";

        public static void Attach(
            SalesReportResponse report,
            IEnumerable<PrincipalSalesOutFakturItemEvidence> lines,
            string selectedSupplierId,
            IEnumerable<PrincipalSalesOutFakturItemEvidenceLine> selectedLines)
        {
            if (report is null)
                throw new ArgumentNullException(nameof(report));

            var period = new Periode(report.PeriodFrom, report.PeriodTo);
            var aggregate = new PrincipalSalesOutAggregator().Aggregate(
                lines,
                period,
                report.GeneratedAt);

            var supplierId = (selectedSupplierId ?? string.Empty).Trim();
            PrincipalSalesOutEvidenceResponse evidence = null;
            if (supplierId.Length > 0)
            {
                evidence = PrincipalSalesOutEvidenceComposer.Compose(
                    supplierId,
                    report.PeriodFrom.Year,
                    report.PeriodFrom.Month,
                    selectedLines);
            }

            report.KpiId = PrincipalKpiCatalog.SalesOutId;
            report.KpiName = KpiName;
            report.HeaderTotalLabel = HeaderTotalLabel;
            report.HeaderTotalAmount = (report.Rows ?? new List<SalesReportRow>())
                .Where(row => row != null)
                .Sum(row => row.FakturTotal);
            report.Disclosures = BuildDisclosures();
            report.Principals = (aggregate.Principals ?? Enumerable.Empty<PrincipalSalesOutRow>())
                .Select(row => new SalesReportPrincipalRow
                {
                    SupplierId = row.SupplierId ?? string.Empty,
                    PrincipalName = string.IsNullOrWhiteSpace(row.SupplierName)
                        ? row.SupplierId ?? string.Empty
                        : row.SupplierName,
                    KpiId = PrincipalKpiCatalog.SalesOutId,
                    PrincipalSalesOutAmount = row.SalesOutAmount,
                    LineCount = row.LineCount
                })
                .ToList();
            report.UnknownPrincipalExceptionCount = (aggregate.DataQuality ?? Enumerable.Empty<PrincipalSalesOutDataQualityRow>())
                .Sum(row => row.LineCount);
            report.SelectedSupplierId = supplierId;
            report.SelectedPrincipalName = evidence?.PrincipalName ?? string.Empty;
            report.SelectedPrincipalSalesOutAmount = evidence?.PrincipalSalesOutAmount ?? 0m;
            report.EvidenceLines = (evidence?.Lines ?? Enumerable.Empty<PrincipalSalesOutEvidenceItem>())
                .Select(line => new SalesReportPrincipalEvidenceLine
                {
                    FakturId = line.FakturId ?? string.Empty,
                    FakturCode = line.FakturCode ?? string.Empty,
                    FakturDate = line.FakturDate,
                    FakturItemId = line.FakturItemId ?? string.Empty,
                    BrgId = line.BrgId ?? string.Empty,
                    SupplierId = line.SupplierId ?? string.Empty,
                    KpiId = PrincipalKpiCatalog.SalesOutId,
                    PrincipalSalesOutAmount = line.PrincipalSalesOutAmount
                })
                .ToList();
        }

        public static IList<string> BuildDisclosures()
        {
            var statements = new List<string>
            {
                MeasureIsPrnSales001AndNotRequiredToEqualGrandTotal,
                ReturnsClaimsAndInventoryAdjustmentsAreNotDeducted,
                CompanyHeaderTotalsAreNotPrincipalSales
            };
            statements.AddRange(PrincipalSalesOutDisclosure.Statements);
            return statements;
        }
    }
}
