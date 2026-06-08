using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using btr.application.ReportingContext.PiutangReportAgg.Contracts;
using btr.application.ReportingContext.Shared;
using btr.application.SupportContext.TglJamAgg;
using MediatR;

namespace btr.application.ReportingContext.PiutangReportAgg.Queries
{
    public class GetPiutangReportQuery : IRequest<PiutangReportResponse>
    {
        public DateTime? From { get; set; }

        public DateTime? To { get; set; }

        public string DateField { get; set; }
    }

    public class PiutangReportResponse
    {
        public DateTime PeriodFrom { get; set; }

        public DateTime PeriodTo { get; set; }

        public string DateField { get; set; }

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
        private readonly ITglJamDal _tglJamDal;

        public GetPiutangReportHandler(IPiutangReportDal dal, ITglJamDal tglJamDal)
        {
            _dal = dal;
            _tglJamDal = tglJamDal;
        }

        public Task<PiutangReportResponse> Handle(
            GetPiutangReportQuery request,
            CancellationToken cancellationToken)
        {
            var periode = ReportPeriodValidator.ResolveAndValidate(
                new ReportPeriodRequest { From = request.From, To = request.To },
                _tglJamDal.Now);
            var dateField = PiutangReportDateFieldParser.Parse(request.DateField);

            return Task.FromResult(_dal.GetReport(periode, dateField));
        }
    }
}
