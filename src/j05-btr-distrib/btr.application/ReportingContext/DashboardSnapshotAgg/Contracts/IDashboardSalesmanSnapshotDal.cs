using System.Collections.Generic;
using btr.application.ReportingContext.DashboardSnapshotAgg.Models;

namespace btr.application.ReportingContext.DashboardSnapshotAgg.Contracts
{
    public interface IDashboardSalesmanSnapshotDal
    {
        DashboardSalesmanAggregateResult GetCurrent();

        void ReplaceCurrent(DashboardSalesmanAggregateResult result, string refreshLogId);

        IList<DashboardSalesmanPrincipalAchievementRow> ListPrincipalAchievement(string salesPersonId);

        IList<DashboardSalesmanRepHistoryRow> ListRepHistory(string salesPersonId, int months);
    }
}
