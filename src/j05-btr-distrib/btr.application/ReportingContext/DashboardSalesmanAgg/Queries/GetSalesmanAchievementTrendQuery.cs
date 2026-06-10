using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using btr.application.ReportingContext.DashboardSalesmanAgg.Contracts;
using MediatR;

namespace btr.application.ReportingContext.DashboardSalesmanAgg.Queries
{
    public class GetSalesmanAchievementTrendQuery : IRequest<SalesmanAchievementTrendResponse>
    {
        public string SalesPersonId { get; set; }

        public int Months { get; set; } = 12;
    }

    public class SalesmanAchievementTrendResponse
    {
        public string SalesPersonId { get; set; }

        public string SalesPersonName { get; set; }

        public IList<SalesmanAchievementTrendPoint> Points { get; set; }
            = new List<SalesmanAchievementTrendPoint>();
    }

    public class SalesmanAchievementTrendPoint
    {
        public int PeriodYear { get; set; }

        public int PeriodMonth { get; set; }

        public string PeriodLabel { get; set; }

        public decimal? TargetAmount { get; set; }

        public decimal CompletedOmzet { get; set; }

        public decimal? AchievementPercent { get; set; }

        public string AchievementBand { get; set; }
    }

    public class GetSalesmanAchievementTrendHandler
        : IRequestHandler<GetSalesmanAchievementTrendQuery, SalesmanAchievementTrendResponse>
    {
        private readonly IDashboardSalesmanDal _dal;

        public GetSalesmanAchievementTrendHandler(IDashboardSalesmanDal dal)
        {
            _dal = dal;
        }

        public Task<SalesmanAchievementTrendResponse> Handle(
            GetSalesmanAchievementTrendQuery request,
            CancellationToken cancellationToken)
        {
            var months = request.Months <= 0 ? 12 : request.Months;
            if (months > 12)
                months = 12;

            return Task.FromResult(_dal.GetAchievementTrend(request.SalesPersonId, months));
        }
    }
}
