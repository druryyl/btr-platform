using System;

namespace btr.application.ReportingContext.DashboardSnapshotAgg
{
    public class DashboardSnapshotUnavailableException : Exception
    {
        public DashboardSnapshotUnavailableException(string message)
            : base(message)
        {
        }
    }
}
