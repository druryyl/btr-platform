using System;
using System.Diagnostics;
using btr.application.ReportingContext.DashboardSnapshotAgg.Contracts;
using btr.application.ReportingContext.DashboardSnapshotAgg.Models;
using btr.application.ReportingContext.DashboardSnapshotAgg.Services;
using btr.application.SupportContext.TglJamAgg;
using btr.nuna.Application;
using btr.nuna.Domain;

namespace btr.application.ReportingContext.DashboardSnapshotAgg.UseCases
{
    public interface IRefreshDashboardPiutangSnapshotWorker
        : INunaServiceVoid<RefreshDashboardPiutangSnapshotRequest>
    {
    }

    public class RefreshDashboardPiutangSnapshotWorker : IRefreshDashboardPiutangSnapshotWorker
    {
        private const string Domain = "Piutang";
        private const int MaxErrorMessageLength = 500;

        private readonly IPiutangOpenBalanceDal _openBalanceDal;
        private readonly DashboardPiutangAggregator _aggregator;
        private readonly IDashboardPiutangSnapshotDal _snapshotDal;
        private readonly IDashboardSnapshotRefreshLogDal _refreshLogDal;
        private readonly ITglJamDal _tglJamDal;

        public RefreshDashboardPiutangSnapshotWorker(
            IPiutangOpenBalanceDal openBalanceDal,
            DashboardPiutangAggregator aggregator,
            IDashboardPiutangSnapshotDal snapshotDal,
            IDashboardSnapshotRefreshLogDal refreshLogDal,
            ITglJamDal tglJamDal)
        {
            _openBalanceDal = openBalanceDal;
            _aggregator = aggregator;
            _snapshotDal = snapshotDal;
            _refreshLogDal = refreshLogDal;
            _tglJamDal = tglJamDal;
        }

        public void Execute(RefreshDashboardPiutangSnapshotRequest request)
        {
            if (request is null)
                throw new ArgumentNullException(nameof(request));

            var sw = Stopwatch.StartNew();
            var refreshLogId = Ulid.NewUlid().ToString();
            var startedAt = _tglJamDal.Now;

            _refreshLogDal.InsertRunning(new DashboardSnapshotRefreshLogModel
            {
                RefreshLogId = refreshLogId,
                Domain = Domain,
                StartedAt = startedAt,
                Status = "Running",
                TriggeredBy = request.TriggeredBy ?? "Scheduler"
            });

            try
            {
                var today = _tglJamDal.Now.Date;
                var generatedAt = _tglJamDal.Now;
                var rows = _openBalanceDal.ListOpenBalances();
                var aggregate = _aggregator.Aggregate(rows, today, generatedAt);

                using (var trans = TransHelper.NewScope())
                {
                    _snapshotDal.ReplaceCurrent(aggregate, refreshLogId);
                    trans.Complete();
                }

                sw.Stop();
                _refreshLogDal.MarkSuccess(refreshLogId, (int)sw.ElapsedMilliseconds);

                request.Result = new RefreshDashboardPiutangSnapshotResult
                {
                    RefreshLogId = refreshLogId,
                    DurationMs = (int)sw.ElapsedMilliseconds
                };
            }
            catch (Exception ex)
            {
                sw.Stop();
                var message = ex.Message ?? ex.GetType().Name;
                if (message.Length > MaxErrorMessageLength)
                    message = message.Substring(0, MaxErrorMessageLength);

                _refreshLogDal.MarkFailed(refreshLogId, (int)sw.ElapsedMilliseconds, message);
                throw;
            }
        }
    }
}
