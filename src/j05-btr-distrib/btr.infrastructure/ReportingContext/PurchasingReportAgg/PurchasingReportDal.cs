using System;
using System.Collections.Generic;
using System.Linq;
using btr.application.PurchaseContext.InvoiceInfo;
using btr.application.ReportingContext.PurchasingReportAgg.Contracts;
using btr.application.ReportingContext.PurchasingReportAgg.Queries;
using btr.application.SupportContext.TglJamAgg;
using btr.nuna.Domain;

namespace btr.infrastructure.ReportingContext.PurchasingReportAgg
{
    public class PurchasingReportDal : IPurchasingReportDal
    {
        private readonly IInvoiceViewDal _invoiceViewDal;
        private readonly ITglJamDal _tglJamDal;

        public PurchasingReportDal(IInvoiceViewDal invoiceViewDal, ITglJamDal tglJamDal)
        {
            _invoiceViewDal = invoiceViewDal;
            _tglJamDal = tglJamDal;
        }

        public PurchasingReportResponse GetReport()
        {
            var periode = CurrentMonthPeriode();
            var sourceRows = _invoiceViewDal.ListData(periode)?.ToList()
                             ?? new List<InvoiceView>();

            var rows = sourceRows
                .OrderBy(r => r.Tgl)
                .ThenBy(r => r.InvoiceCode)
                .Select(MapRow)
                .ToList();

            var summary = new PurchasingReportSummary
            {
                GrandTotalPurchase = rows.Sum(r => r.GrandTotal),
                TotalInvoice = rows.Count,
            };

            return new PurchasingReportResponse
            {
                PeriodFrom = periode.Tgl1,
                PeriodTo = periode.Tgl2,
                GeneratedAt = _tglJamDal.Now,
                Summary = summary,
                Rows = rows,
            };
        }

        private static PurchasingReportRow MapRow(InvoiceView row)
        {
            return new PurchasingReportRow
            {
                InvoiceCode = row.InvoiceCode ?? string.Empty,
                InvoiceDate = row.Tgl.Date,
                SupplierName = row.SupplierName ?? string.Empty,
                WarehouseName = row.WarehouseName ?? string.Empty,
                Total = row.Total,
                Disc = row.Disc,
                Tax = row.Tax,
                GrandTotal = row.GrandTotal,
                PostingStok = row.PostingStok ?? string.Empty,
            };
        }

        private Periode CurrentMonthPeriode()
        {
            var today = _tglJamDal.Now.Date;
            var monthStart = new DateTime(today.Year, today.Month, 1);
            var monthEnd = new DateTime(today.Year, today.Month, DateTime.DaysInMonth(today.Year, today.Month));
            return new Periode(monthStart, monthEnd);
        }
    }
}
