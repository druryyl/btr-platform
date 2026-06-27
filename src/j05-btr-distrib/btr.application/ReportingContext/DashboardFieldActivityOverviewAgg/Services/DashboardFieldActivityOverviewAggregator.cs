using System;
using btr.application.ReportingContext.DashboardFieldActivityOverviewAgg.Models;
using btr.application.SupportContext.TglJamAgg;

namespace btr.application.ReportingContext.DashboardFieldActivityOverviewAgg.Services
{
    public class DashboardFieldActivityOverviewAggregator
    {
        private readonly FieldActivityOverviewComposer _composer;
        private readonly ITglJamDal _tglJamDal;

        public DashboardFieldActivityOverviewAggregator(
            FieldActivityOverviewComposer composer,
            ITglJamDal tglJamDal)
        {
            _composer = composer;
            _tglJamDal = tglJamDal;
        }

        public DashboardFieldActivityAggregateResult Aggregate(DateTime activityDate)
        {
            return _composer.BuildAggregate(activityDate.Date, _tglJamDal.Now, includeTrends: true);
        }
    }
}
