using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using btr.application.ReportingContext.PiutangReportAgg.Contracts;
using MediatR;

namespace btr.application.ReportingContext.PiutangReportAgg.Queries
{
    public class GetPiutangReportQuery : IRequest<PiutangReportResponse>
    {
    }

    public class PiutangReportResponse
    {
        public DateTime PeriodFrom { get; set; }

        public DateTime PeriodTo { get; set; }

        public DateTime GeneratedAt { get; set; }

        public PiutangReportSummary Summary { get; set; } = new PiutangReportSummary();

        public List<PiutangReportRow> Rows { get; set; } = new List<PiutangReportRow>();
    }

    public class PiutangReportSummary
    {
        public decimal TotalPiutang { get; set; }

        public int TotalCustomer { get; set; }
    }

    public class PiutangReportRow
    {
        public string CustomerName { get; set; }

        public string SalesName { get; set; }

        public string FakturCode { get; set; }

        public DateTime FakturDate { get; set; }

        public DateTime JatuhTempo { get; set; }

        public decimal TotalJual { get; set; }

        public decimal KurangBayar { get; set; }
    }

    public class GetPiutangReportHandler
        : IRequestHandler<GetPiutangReportQuery, PiutangReportResponse>
    {
        private readonly IPiutangReportDal _dal;

        public GetPiutangReportHandler(IPiutangReportDal dal)
        {
            _dal = dal;
        }

        public Task<PiutangReportResponse> Handle(
            GetPiutangReportQuery request,
            CancellationToken cancellationToken)
        {
            return Task.FromResult(_dal.GetReport());
        }
    }
}
