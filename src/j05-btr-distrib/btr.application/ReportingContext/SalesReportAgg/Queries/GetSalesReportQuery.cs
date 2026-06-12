using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
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
    }

    public class SalesReportResponse
    {
        public DateTime PeriodFrom { get; set; }

        public DateTime PeriodTo { get; set; }

        public DateTime GeneratedAt { get; set; }

        public List<SalesReportRow> Rows { get; set; } = new List<SalesReportRow>();
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

    public class GetSalesReportHandler
        : IRequestHandler<GetSalesReportQuery, SalesReportResponse>
    {
        private readonly ISalesReportDal _dal;
        private readonly IBusinessDateProvider _businessDateProvider;

        public GetSalesReportHandler(ISalesReportDal dal, IBusinessDateProvider businessDateProvider)
        {
            _dal = dal;
            _businessDateProvider = businessDateProvider;
        }

        public Task<SalesReportResponse> Handle(
            GetSalesReportQuery request,
            CancellationToken cancellationToken)
        {
            var periode = ReportPeriodValidator.ResolveAndValidate(
                new ReportPeriodRequest { From = request.From, To = request.To },
                _businessDateProvider.Today);

            return Task.FromResult(_dal.GetReport(periode));
        }
    }
}
