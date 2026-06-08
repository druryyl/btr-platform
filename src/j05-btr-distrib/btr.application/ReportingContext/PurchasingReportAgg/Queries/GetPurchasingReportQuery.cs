using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using btr.application.ReportingContext.PurchasingReportAgg.Contracts;
using btr.application.ReportingContext.Shared;
using btr.application.SupportContext.TglJamAgg;
using MediatR;

namespace btr.application.ReportingContext.PurchasingReportAgg.Queries
{
    public class GetPurchasingReportQuery : IRequest<PurchasingReportResponse>
    {
        public DateTime? From { get; set; }

        public DateTime? To { get; set; }
    }

    public class PurchasingReportResponse
    {
        public DateTime PeriodFrom { get; set; }

        public DateTime PeriodTo { get; set; }

        public DateTime GeneratedAt { get; set; }

        public PurchasingReportSummary Summary { get; set; } = new PurchasingReportSummary();

        public List<PurchasingReportRow> Rows { get; set; } = new List<PurchasingReportRow>();
    }

    public class PurchasingReportSummary
    {
        public decimal GrandTotalPurchase { get; set; }

        public int TotalInvoice { get; set; }
    }

    public class PurchasingReportRow
    {
        public string InvoiceCode { get; set; }

        public DateTime InvoiceDate { get; set; }

        public string SupplierName { get; set; }

        public string WarehouseName { get; set; }

        public decimal Total { get; set; }

        public decimal Disc { get; set; }

        public decimal Tax { get; set; }

        public decimal GrandTotal { get; set; }

        public string PostingStok { get; set; }
    }

    public class GetPurchasingReportHandler
        : IRequestHandler<GetPurchasingReportQuery, PurchasingReportResponse>
    {
        private readonly IPurchasingReportDal _dal;
        private readonly ITglJamDal _tglJamDal;

        public GetPurchasingReportHandler(IPurchasingReportDal dal, ITglJamDal tglJamDal)
        {
            _dal = dal;
            _tglJamDal = tglJamDal;
        }

        public Task<PurchasingReportResponse> Handle(
            GetPurchasingReportQuery request,
            CancellationToken cancellationToken)
        {
            var periode = ReportPeriodValidator.ResolveAndValidate(
                new ReportPeriodRequest { From = request.From, To = request.To },
                _tglJamDal.Now);

            return Task.FromResult(_dal.GetReport(periode));
        }
    }
}

