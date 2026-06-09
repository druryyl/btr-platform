using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using btr.application.ReportingContext.DashboardExecutiveAgg.Contracts;
using btr.application.ReportingContext.Shared;
using MediatR;

namespace btr.application.ReportingContext.DashboardExecutiveAgg.Queries
{
    public class GetDashboardExecutiveQuery : IRequest<DashboardExecutiveResponse>
    {
    }

    public class DashboardExecutiveResponse
    {
        public bool HasUnavailableDomain { get; set; }

        public bool IsDataFresh { get; set; }

        public DateTime? LastRefreshed { get; set; }

        public string OverallHealthStatus { get; set; }

        public DashboardExecutiveSalesAttention Sales { get; set; }

        public DashboardExecutivePiutangAttention Piutang { get; set; }

        public DashboardExecutivePurchasingAttention Purchasing { get; set; }

        public DashboardExecutiveInventoryAttention Inventory { get; set; }

        public DashboardExecutiveCriticalExposures CriticalExposures { get; set; }

        public IList<DashboardExecutiveDomainSummary> DomainSummaries { get; set; }
    }

    public class DashboardExecutiveSalesAttention
    {
        public decimal? AchievementPercent { get; set; }

        public decimal TotalAchievement { get; set; }

        public string AchievementBand { get; set; }

        public bool RequiresAttention { get; set; }

        public bool IsAvailable { get; set; }
    }

    public class DashboardExecutivePiutangAttention
    {
        public decimal TotalPiutang { get; set; }

        public int OverdueCustomer { get; set; }

        public decimal AgingOver90Amount { get; set; }

        public decimal? AgingOver90Percent { get; set; }

        public decimal? TopCustomerPercent { get; set; }

        public bool RequiresAttention { get; set; }

        public bool IsAvailable { get; set; }
    }

    public class DashboardExecutivePurchasingAttention
    {
        public int PendingPostingInvoiceCount { get; set; }

        public decimal PendingPostingValue { get; set; }

        public int QualifiedBacklogCount { get; set; }

        public decimal? TopPrincipalPercent { get; set; }

        public bool RequiresAttention { get; set; }

        public bool IsAvailable { get; set; }
    }

    public class DashboardExecutiveInventoryAttention
    {
        public decimal TotalInventoryValue { get; set; }

        public decimal? TopCategoryPercent { get; set; }

        public decimal? TopSupplierPercent { get; set; }

        public bool RequiresAttention { get; set; }

        public bool IsAvailable { get; set; }
    }

    public class DashboardExecutiveCriticalExposures
    {
        public IList<DashboardExecutiveRiskItem> TopCustomers { get; set; }

        public IList<DashboardExecutiveRiskItem> TopCategories { get; set; }

        public IList<DashboardExecutiveRiskItem> TopSuppliers { get; set; }

        public IList<DashboardExecutiveRiskItem> TopPrincipals { get; set; }
    }

    public class DashboardExecutiveRiskItem
    {
        public int Rank { get; set; }

        public string Name { get; set; }

        public decimal Amount { get; set; }

        public InvestigationMetadata Investigation { get; set; }
    }

    public class DashboardExecutiveDomainSummary
    {
        public string Domain { get; set; }

        public string SummaryText { get; set; }

        public string DetailDashboardRoute { get; set; }

        public bool IsAvailable { get; set; }
    }

    public class GetDashboardExecutiveHandler
        : IRequestHandler<GetDashboardExecutiveQuery, DashboardExecutiveResponse>
    {
        private readonly IDashboardExecutiveDal _dal;

        public GetDashboardExecutiveHandler(IDashboardExecutiveDal dal)
        {
            _dal = dal;
        }

        public Task<DashboardExecutiveResponse> Handle(
            GetDashboardExecutiveQuery request,
            CancellationToken cancellationToken)
        {
            return Task.FromResult(_dal.GetExecutive());
        }
    }
}
