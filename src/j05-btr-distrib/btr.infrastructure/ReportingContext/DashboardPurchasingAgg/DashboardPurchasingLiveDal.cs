using System;
using System.Collections.Generic;
using System.Linq;
using btr.application.PurchaseContext.InvoiceInfo;
using btr.application.ReportingContext.DashboardPurchasingAgg.Queries;
using btr.application.ReportingContext.DashboardSnapshotAgg.Models;
using btr.application.ReportingContext.DashboardSnapshotAgg.Services;
using btr.application.SupportContext.TglJamAgg;
using btr.infrastructure.ReportingContext.DashboardSnapshotAgg;
using btr.nuna.Domain;

namespace btr.infrastructure.ReportingContext.DashboardPurchasingAgg
{
    public class DashboardPurchasingLiveDal
    {
        private readonly IInvoiceViewDal _invoiceViewDal;
        private readonly ITglJamDal _tglJamDal;
        private readonly DashboardPurchasingInvoiceAggregator _aggregator;

        public DashboardPurchasingLiveDal(
            IInvoiceViewDal invoiceViewDal,
            ITglJamDal tglJamDal,
            DashboardPurchasingInvoiceAggregator aggregator)
        {
            _invoiceViewDal = invoiceViewDal;
            _tglJamDal = tglJamDal;
            _aggregator = aggregator;
        }

        public DashboardPurchasingResponse GetSummary()
        {
            var today = _tglJamDal.Now.Date;
            var periode = CurrentMonthPeriode(today);
            var rows = _invoiceViewDal.ListData(periode)?.ToList()
                       ?? new List<InvoiceView>();
            var aggregate = _aggregator.Aggregate(rows, periode, _tglJamDal.Now);
            return DashboardPurchasingSnapshotDal.MapToResponse(aggregate);
        }

        private static Periode CurrentMonthPeriode(DateTime today)
        {
            var monthStart = new DateTime(today.Year, today.Month, 1);
            var monthEnd = new DateTime(today.Year, today.Month, DateTime.DaysInMonth(today.Year, today.Month));
            return new Periode(monthStart, monthEnd);
        }
    }
}
