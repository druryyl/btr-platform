using System;
using System.Diagnostics;
using btr.application.Portal;
using btr.application.ReportingContext.DashboardSnapshotAgg.Contracts;
using btr.application.ReportingContext.DashboardSnapshotAgg.Models;
using btr.application.ReportingContext.DashboardSnapshotAgg.Progress;
using btr.application.ReportingContext.PrincipalAnalyticsAgg.Contracts;
using btr.application.ReportingContext.PrincipalAnalyticsAgg.Services;
using btr.application.SupportContext.TglJamAgg;
using btr.nuna.Application;
using btr.nuna.Domain;

namespace btr.application.ReportingContext.PrincipalAnalyticsAgg.UseCases
{
    public interface IRefreshPrincipalSalesmanContributionSnapshotWorker
        : INunaServiceVoid<RefreshPrincipalSalesmanContributionSnapshotRequest>
    {
    }

    public class RefreshPrincipalSalesmanContributionSnapshotWorker : IRefreshPrincipalSalesmanContributionSnapshotWorker
    {
        private const int MaxErrorMessageLength = 500;

        private readonly IPrincipalContributionEvidenceDal _evidenceDal;
        private readonly IPrincipalTargetEvidenceDal _targetEvidenceDal;
        private readonly PrincipalSalesmanContributionComposer _composer;
        private readonly IPrincipalSalesmanContributionSnapshotDal _snapshotDal;
        private readonly IDashboardSnapshotRefreshLogDal _refreshLogDal;
        private readonly ITglJamDal _tglJamDal;
        private readonly IBusinessDateProvider _businessDateProvider;

        public RefreshPrincipalSalesmanContributionSnapshotWorker(
            IPrincipalContributionEvidenceDal evidenceDal,
            IPrincipalTargetEvidenceDal targetEvidenceDal,
            PrincipalSalesmanContributionComposer composer,
            IPrincipalSalesmanContributionSnapshotDal snapshotDal,
            IDashboardSnapshotRefreshLogDal refreshLogDal,
            ITglJamDal tglJamDal,
            IBusinessDateProvider businessDateProvider)
        {
            _evidenceDal = evidenceDal;
            _targetEvidenceDal = targetEvidenceDal;
            _composer = composer;
            _snapshotDal = snapshotDal;
            _refreshLogDal = refreshLogDal;
            _tglJamDal = tglJamDal;
            _businessDateProvider = businessDateProvider;
        }

        public void Execute(RefreshPrincipalSalesmanContributionSnapshotRequest request)
        {
            if (request is null)
                throw new ArgumentNullException(nameof(request));

            var sw = Stopwatch.StartNew();
            var refreshLogId = Ulid.NewUlid().ToString();
            var startedAt = _tglJamDal.Now;
            var domain = PrincipalSalesmanContributionSnapshot.Domain;

            WorkerProgressScope.Current.StepStarted($"{domain}:Initialize", "Initialize refresh log");
            _refreshLogDal.InsertRunning(new DashboardSnapshotRefreshLogModel
            {
                RefreshLogId = refreshLogId,
                Domain = domain,
                StartedAt = startedAt,
                Status = "Running",
                TriggeredBy = request.TriggeredBy ?? "Scheduler"
            });
            WorkerProgressScope.Current.StepCompleted($"{domain}:Initialize");

            try
            {
                var today = _businessDateProvider.Today;
                var periode = CurrentMonthPeriode(today);
                var generatedAt = _tglJamDal.Now;

                WorkerProgressScope.Current.StepStarted($"{domain}:Load", "Load contribution evidence");
                var lines = _evidenceDal.ListContributionEvidence(periode);
                WorkerProgressScope.Current.StepCompleted($"{domain}:Load", new WorkerProgressStepInfo
                {
                    RecordCount = lines?.Count ?? 0
                });

                WorkerProgressScope.Current.StepStarted($"{domain}:LoadTargets", "Load Salesman Principal Targets");
                var targets = _targetEvidenceDal.ListSalesmanPrincipalTargets(periode.Tgl1.Year, periode.Tgl1.Month);
                WorkerProgressScope.Current.StepCompleted($"{domain}:LoadTargets");

                WorkerProgressScope.Current.StepStarted($"{domain}:Compose", "Compose contribution and responsibility exceptions");
                var contribution = _composer.Compose(lines, targets, periode.Tgl1.Year, periode.Tgl1.Month, generatedAt);
                WorkerProgressScope.Current.StepCompleted($"{domain}:Compose");

                WorkerProgressScope.Current.StepStarted($"{domain}:Save", "Save contribution snapshot");
                using (var trans = TransHelper.NewScope())
                {
                    _snapshotDal.ReplaceCurrent(contribution, refreshLogId);
                    trans.Complete();
                }
                WorkerProgressScope.Current.StepCompleted($"{domain}:Save");

                sw.Stop();
                _refreshLogDal.MarkSuccess(refreshLogId, (int)sw.ElapsedMilliseconds);

                request.Result = new RefreshPrincipalSalesmanContributionSnapshotResult
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
                WorkerProgressScope.Current.StepFailed($"{domain}:Execute", message);
                throw;
            }
        }

        private static Periode CurrentMonthPeriode(DateTime today)
        {
            var monthStart = new DateTime(today.Year, today.Month, 1);
            var monthEnd = new DateTime(today.Year, today.Month, DateTime.DaysInMonth(today.Year, today.Month));
            return new Periode(monthStart, monthEnd);
        }
    }
}
