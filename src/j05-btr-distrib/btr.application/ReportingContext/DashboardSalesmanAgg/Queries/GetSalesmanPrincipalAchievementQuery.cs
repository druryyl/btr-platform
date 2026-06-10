using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using btr.application.ReportingContext.DashboardSalesmanAgg.Contracts;
using MediatR;

namespace btr.application.ReportingContext.DashboardSalesmanAgg.Queries
{
    public class GetSalesmanPrincipalAchievementQuery : IRequest<SalesmanPrincipalAchievementResponse>
    {
        public string SalesPersonId { get; set; }
    }

    public class SalesmanPrincipalAchievementResponse
    {
        public string SalesPersonId { get; set; }

        public string SalesPersonName { get; set; }

        public int PeriodYear { get; set; }

        public int PeriodMonth { get; set; }

        public IList<SalesmanPrincipalAchievementRow> Principals { get; set; }
            = new List<SalesmanPrincipalAchievementRow>();
    }

    public class SalesmanPrincipalAchievementRow
    {
        public string SupplierId { get; set; }

        public string SupplierName { get; set; }

        public decimal? TargetAmount { get; set; }

        public decimal CompletedOmzet { get; set; }

        public decimal? AchievementPercent { get; set; }

        public string AchievementBand { get; set; }
    }

    public class GetSalesmanPrincipalAchievementHandler
        : IRequestHandler<GetSalesmanPrincipalAchievementQuery, SalesmanPrincipalAchievementResponse>
    {
        private readonly IDashboardSalesmanDal _dal;

        public GetSalesmanPrincipalAchievementHandler(IDashboardSalesmanDal dal)
        {
            _dal = dal;
        }

        public Task<SalesmanPrincipalAchievementResponse> Handle(
            GetSalesmanPrincipalAchievementQuery request,
            CancellationToken cancellationToken)
        {
            return Task.FromResult(_dal.GetPrincipalAchievement(request.SalesPersonId));
        }
    }
}
