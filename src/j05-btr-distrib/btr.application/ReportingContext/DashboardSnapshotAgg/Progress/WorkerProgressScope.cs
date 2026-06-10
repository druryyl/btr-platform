using System;

namespace btr.application.ReportingContext.DashboardSnapshotAgg.Progress
{
    public static class WorkerProgressScope
    {
        [ThreadStatic]
        private static IWorkerProgressReporter _current;

        public static IWorkerProgressReporter Current =>
            _current ?? NullWorkerProgressReporter.Instance;

        public static IDisposable Push(IWorkerProgressReporter reporter)
        {
            if (reporter is null)
                throw new ArgumentNullException(nameof(reporter));

            return new Scope(reporter);
        }

        private sealed class Scope : IDisposable
        {
            private readonly IWorkerProgressReporter _previous;
            private bool _disposed;

            public Scope(IWorkerProgressReporter reporter)
            {
                _previous = _current;
                _current = reporter;
            }

            public void Dispose()
            {
                if (_disposed)
                    return;

                _current = _previous;
                _disposed = true;
            }
        }
    }
}
