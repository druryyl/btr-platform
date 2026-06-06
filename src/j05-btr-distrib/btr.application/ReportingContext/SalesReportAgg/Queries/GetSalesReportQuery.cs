using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using btr.application.ReportingContext.SalesReportAgg.Contracts;
using MediatR;

namespace btr.application.ReportingContext.SalesReportAgg.Queries
{
    public class GetSalesReportQuery : IRequest<SalesReportResponse>
    {
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

        public string SalesName { get; set; }

        public decimal FakturTotal { get; set; }

        public string Status { get; set; }
    }

    public class GetSalesReportHandler
        : IRequestHandler<GetSalesReportQuery, SalesReportResponse>
    {
        private readonly ISalesReportDal _dal;

        public GetSalesReportHandler(ISalesReportDal dal)
        {
            _dal = dal;
        }

        public Task<SalesReportResponse> Handle(
            GetSalesReportQuery request,
            CancellationToken cancellationToken)
        {
            return Task.FromResult(_dal.GetReport());
        }
    }
}
