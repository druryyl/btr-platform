using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using btr.application.ReportingContext.CustomerReportAgg.Contracts;
using MediatR;

namespace btr.application.ReportingContext.CustomerReportAgg.Queries
{
    public class GetCustomerReportQuery : IRequest<CustomerReportResponse>
    {
        public string CustomerCode { get; set; }
    }

    public class CustomerReportResponse
    {
        public bool IsAvailable { get; set; }

        public DateTime GeneratedAt { get; set; }

        public DateTime BusinessDate { get; set; }

        public CustomerReportSummaryDto Summary { get; set; } = new CustomerReportSummaryDto();

        public IReadOnlyList<CustomerReportRowDto> Rows { get; set; } = new List<CustomerReportRowDto>();
    }

    public class CustomerReportSummaryDto
    {
        public int TotalCustomers { get; set; }

        public decimal TotalMtdOmzet { get; set; }

        public decimal TotalOpenBalance { get; set; }

        public string ValueDisclaimerText { get; set; }
    }

    public class CustomerReportRowDto
    {
        public int SortOrder { get; set; }

        public string CustomerCode { get; set; }

        public string CustomerName { get; set; }

        public string WilayahName { get; set; }

        public string Klasifikasi { get; set; }

        public string LifecycleStage { get; set; }

        public string LifecycleLabel { get; set; }

        public string PortfolioTier { get; set; }

        public string TierLabel { get; set; }

        public string PrimaryActionKey { get; set; }

        public string PrimaryActionLabel { get; set; }

        public string ActionOwner { get; set; }

        public string ActionReasonText { get; set; }

        public decimal MtdOmzet { get; set; }

        public decimal OpenBalance { get; set; }

        public decimal? OverdueBalance { get; set; }

        public DateTime? LastPurchaseDate { get; set; }

        public DateTime? FirstPurchaseDate { get; set; }

        public string M29Category { get; set; }

        public string SalesPersonName { get; set; }

        public decimal? SalesmanAchievementPercent { get; set; }

        public bool IsAttention { get; set; }

        public string ValueDisclaimer { get; set; }
    }

    public class GetCustomerReportHandler
        : IRequestHandler<GetCustomerReportQuery, CustomerReportResponse>
    {
        private readonly ICustomerReportDal _dal;

        public GetCustomerReportHandler(ICustomerReportDal dal)
        {
            _dal = dal;
        }

        public Task<CustomerReportResponse> Handle(
            GetCustomerReportQuery request,
            CancellationToken cancellationToken)
        {
            return Task.FromResult(_dal.GetReport(request.CustomerCode));
        }
    }
}
