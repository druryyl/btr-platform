using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using btr.application.ReportingContext.InventoryReportAgg.Contracts;
using MediatR;

namespace btr.application.ReportingContext.InventoryReportAgg.Queries
{
    public class GetInventoryReportQuery : IRequest<InventoryReportResponse>
    {
    }

    public class InventoryReportResponse
    {
        public DateTime GeneratedAt { get; set; }

        public InventoryReportSummary Summary { get; set; } = new InventoryReportSummary();

        public List<InventoryReportRow> Rows { get; set; } = new List<InventoryReportRow>();
    }

    public class InventoryReportSummary
    {
        public decimal TotalInventoryValue { get; set; }

        public int TotalItem { get; set; }
    }

    public class InventoryReportRow
    {
        public string BrgId { get; set; }

        public string ItemDisplay { get; set; }

        public string WarehouseName { get; set; }

        public int Qty { get; set; }

        public decimal Hpp { get; set; }

        public decimal NilaiSediaan { get; set; }
    }

    public class GetInventoryReportHandler
        : IRequestHandler<GetInventoryReportQuery, InventoryReportResponse>
    {
        private readonly IInventoryReportDal _dal;

        public GetInventoryReportHandler(IInventoryReportDal dal)
        {
            _dal = dal;
        }

        public Task<InventoryReportResponse> Handle(
            GetInventoryReportQuery request,
            CancellationToken cancellationToken)
        {
            return Task.FromResult(_dal.GetReport());
        }
    }
}
