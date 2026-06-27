using System;
using System.Threading;
using System.Threading.Tasks;
using btr.application.Portal;
using System.Globalization;
using btr.application.ReportingContext.DashboardFieldActivityAgg.Services;
using btr.application.ReportingContext.DashboardFieldActivityOverviewAgg.Contracts;
using btr.application.ReportingContext.DashboardFieldActivityOverviewAgg.Models;
using btr.application.ReportingContext.DashboardFieldActivityOverviewAgg.Services;
using btr.application.SupportContext.TglJamAgg;
using MediatR;

namespace btr.application.ReportingContext.DashboardFieldActivityOverviewAgg.Queries
{
    public class GetFieldActivityOverviewQuery : IRequest<FieldActivityOverviewResponse>
    {
        public DateTime? VisitDate { get; set; }
    }

    public class GetFieldActivityOverviewQueryHandler
        : IRequestHandler<GetFieldActivityOverviewQuery, FieldActivityOverviewResponse>
    {
        private readonly FieldActivityOverviewComposer _composer;
        private readonly IDashboardFieldActivitySnapshotDal _snapshotDal;
        private readonly IBusinessDateProvider _businessDateProvider;
        private readonly ITglJamDal _tglJamDal;
        private readonly FieldActivityOptions _options;

        public GetFieldActivityOverviewQueryHandler(
            FieldActivityOverviewComposer composer,
            IDashboardFieldActivitySnapshotDal snapshotDal,
            IBusinessDateProvider businessDateProvider,
            ITglJamDal tglJamDal,
            FieldActivityOptions options)
        {
            _composer = composer;
            _snapshotDal = snapshotDal;
            _businessDateProvider = businessDateProvider;
            _tglJamDal = tglJamDal;
            _options = options ?? new FieldActivityOptions();
        }

        public Task<FieldActivityOverviewResponse> Handle(
            GetFieldActivityOverviewQuery request,
            CancellationToken cancellationToken)
        {
            var visitDay = (request.VisitDate ?? _businessDateProvider.Today).Date;
            var today = _businessDateProvider.Today.Date;

            if (visitDay == today)
            {
                var snapshot = _snapshotDal.GetCurrent();
                if (snapshot != null && snapshot.ActivityDate.Date == today)
                {
                    var response = _composer.MapToResponse(snapshot, "Snapshot", null);
                    EnrichMeta(response, visitDay);
                    return Task.FromResult(response);
                }
            }

            var queriedAt = _tglJamDal.Now;
            var live = _composer.Compose(visitDay, queriedAt);
            EnrichMeta(live, visitDay);
            return Task.FromResult(live);
        }

        private void EnrichMeta(FieldActivityOverviewResponse response, DateTime visitDay)
        {
            if (response == null)
                return;

            response.Meta = new FieldActivityOverviewMeta
            {
                PlanDataAvailable = visitDay >= _options.VisitPlanGoLiveDate.Date,
                VisitPlanGoLiveDate = _options.VisitPlanGoLiveDate.ToString(
                    "yyyy-MM-dd", CultureInfo.InvariantCulture)
            };
        }
    }
}
