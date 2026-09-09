using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using btr.application.ReportingContext.PrincipalAnalyticsAgg.Contracts;
using btr.application.ReportingContext.SalesReportAgg.Contracts;
using btr.application.ReportingContext.Shared;
using btr.application.Portal;
using MediatR;

namespace btr.application.ReportingContext.SalesReportAgg.Queries
{
    public class GetSalesReportQuery : IRequest<SalesReportResponse>
    {
        public DateTime? From { get; set; }

        public DateTime? To { get; set; }

        public string SupplierId { get; set; }
    }

    public class SalesReportResponse
    {
        public DateTime PeriodFrom { get; set; }

        public DateTime PeriodTo { get; set; }

        public DateTime GeneratedAt { get; set; }

        public List<SalesReportRow> Rows { get; set; } = new List<SalesReportRow>();

        public string KpiId { get; set; }

        public string KpiName { get; set; }

        public string HeaderTotalLabel { get; set; }

        public decimal HeaderTotalAmount { get; set; }

        public IList<string> Disclosures { get; set; } = new List<string>();

        public IList<SalesReportPrincipalRow> Principals { get; set; }
            = new List<SalesReportPrincipalRow>();

        public int UnknownPrincipalExceptionCount { get; set; }

        public string SelectedSupplierId { get; set; }

        public string SelectedPrincipalName { get; set; }

        public decimal SelectedPrincipalSalesOutAmount { get; set; }

        public IList<SalesReportPrincipalEvidenceLine> EvidenceLines { get; set; }
            = new List<SalesReportPrincipalEvidenceLine>();
    }

    public class SalesReportRow
    {
        public DateTime FakturDate { get; set; }

        public string FakturCode { get; set; }

        public string CustomerName { get; set; }

        public string SalesPersonId { get; set; }

        public string SalesName { get; set; }

        public decimal FakturTotal { get; set; }

        public string Status { get; set; }
    }

    public class SalesReportPrincipalRow
    {
        public string SupplierId { get; set; }

        public string PrincipalName { get; set; }

        public string KpiId { get; set; }

        public decimal PrincipalSalesOutAmount { get; set; }

        public int LineCount { get; set; }
    }

    public class SalesReportPrincipalEvidenceLine
    {
        public string FakturId { get; set; }

        public string FakturCode { get; set; }

        public DateTime FakturDate { get; set; }

        public string FakturItemId { get; set; }

        public string BrgId { get; set; }

        public string SupplierId { get; set; }

        public string KpiId { get; set; }

        public decimal PrincipalSalesOutAmount { get; set; }
    }

    public class GetSalesReportHandler
        : IRequestHandler<GetSalesReportQuery, SalesReportResponse>
    {
        private readonly ISalesReportDal _dal;
        private readonly IPrincipalSalesOutEvidenceDal _evidenceDal;
        private readonly IBusinessDateProvider _businessDateProvider;

        public GetSalesReportHandler(
            ISalesReportDal dal,
            IPrincipalSalesOutEvidenceDal evidenceDal,
            IBusinessDateProvider businessDateProvider)
        {
            _dal = dal;
            _evidenceDal = evidenceDal;
            _businessDateProvider = businessDateProvider;
        }

        public Task<SalesReportResponse> Handle(
            GetSalesReportQuery request,
            CancellationToken cancellationToken)
        {
            var periode = ReportPeriodValidator.ResolveAndValidate(
                new ReportPeriodRequest { From = request.From, To = request.To },
                _businessDateProvider.Today);

            var report = _dal.GetReport(periode);
            var supplierId = (request?.SupplierId ?? string.Empty).Trim();
            var lines = _evidenceDal.ListFakturItemEvidence(periode);
            var selectedLines = supplierId.Length == 0
                ? new List<PrincipalSalesOutFakturItemEvidenceLine>()
                : _evidenceDal.ListFakturItemEvidenceForPrincipal(periode, supplierId);

            SalesReportPrincipalEvidenceComposer.Attach(report, lines, supplierId, selectedLines);
            return Task.FromResult(report);
        }
    }
}
