using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using btr.application.ReportingContext.CustomerReportAgg.Contracts;
using btr.application.ReportingContext.PrincipalAnalyticsAgg.Contracts;
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

        public CustomerReportPrincipalPairEvidence PairEvidence { get; set; }
            = new CustomerReportPrincipalPairEvidence();
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

    public class CustomerReportPrincipalPairEvidence
    {
        public bool IsAvailable { get; set; }

        public string KpiId { get; set; }

        public string Note { get; set; }

        public IList<string> Disclosures { get; set; }
            = new List<string>();

        public IList<CustomerReportPrincipalPairCustomer> Customers { get; set; }
            = new List<CustomerReportPrincipalPairCustomer>();
    }

    public class CustomerReportPrincipalPairCustomer
    {
        public string CustomerCode { get; set; }

        public string CustomerName { get; set; }

        public IList<CustomerReportPrincipalPair> Principals { get; set; }
            = new List<CustomerReportPrincipalPair>();
    }

    public class CustomerReportPrincipalPair
    {
        public string SupplierId { get; set; }

        public string PrincipalName { get; set; }

        public string RelationshipStatus { get; set; }

        public string KpiId { get; set; }

        public decimal PairSalesOutAmount { get; set; }
    }

    public class GetCustomerReportHandler
        : IRequestHandler<GetCustomerReportQuery, CustomerReportResponse>
    {
        private readonly ICustomerReportDal _dal;
        private readonly ICustomerPrincipalRelationshipDal _relationshipDal;

        public GetCustomerReportHandler(
            ICustomerReportDal dal,
            ICustomerPrincipalRelationshipDal relationshipDal)
        {
            _dal = dal;
            _relationshipDal = relationshipDal;
        }

        public Task<CustomerReportResponse> Handle(
            GetCustomerReportQuery request,
            CancellationToken cancellationToken)
        {
            var response = _dal.GetReport(request.CustomerCode);
            var customerCodes = CustomerReportPrincipalPairComposer.CollectReportCustomerCodes(response);
            response.PairEvidence = CustomerReportPrincipalPairComposer.Compose(
                response,
                _relationshipDal.ListPairsForCustomerCodes(customerCodes));
            return Task.FromResult(response);
        }
    }
}
