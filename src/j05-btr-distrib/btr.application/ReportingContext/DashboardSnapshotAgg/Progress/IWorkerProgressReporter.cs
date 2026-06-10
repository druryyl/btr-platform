using System.Collections.Generic;

namespace btr.application.ReportingContext.DashboardSnapshotAgg.Progress
{
    public interface IWorkerProgressReporter
    {
        void BeginPlan(IReadOnlyList<string> stepIds, IReadOnlyList<string> displayNames);

        void StepStarted(string stepId, string displayName);

        void StepCompleted(string stepId, WorkerProgressStepInfo info = null);

        void StepFailed(string stepId, string message);

        void ReportPhaseProgress(string label, int current, int total);

        void ReportHeartbeat(string message);
    }
}
