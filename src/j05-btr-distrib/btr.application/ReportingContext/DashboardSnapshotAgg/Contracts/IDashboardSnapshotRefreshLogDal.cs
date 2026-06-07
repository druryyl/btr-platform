using btr.application.ReportingContext.DashboardSnapshotAgg.Models;

namespace btr.application.ReportingContext.DashboardSnapshotAgg.Contracts
{
    public interface IDashboardSnapshotRefreshLogDal
    {
        void InsertRunning(DashboardSnapshotRefreshLogModel model);

        void MarkSuccess(string refreshLogId, int durationMs);

        void MarkFailed(string refreshLogId, int durationMs, string errorMessage);
    }
}
