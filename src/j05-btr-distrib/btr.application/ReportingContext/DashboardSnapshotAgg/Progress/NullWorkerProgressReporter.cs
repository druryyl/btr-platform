using System.Collections.Generic;

namespace btr.application.ReportingContext.DashboardSnapshotAgg.Progress
{
    public sealed class NullWorkerProgressReporter : IWorkerProgressReporter
    {
        public static readonly NullWorkerProgressReporter Instance = new NullWorkerProgressReporter();

        private NullWorkerProgressReporter()
        {
        }

        public void BeginPlan(IReadOnlyList<string> stepIds, IReadOnlyList<string> displayNames)
        {
        }

        public void StepStarted(string stepId, string displayName)
        {
        }

        public void StepCompleted(string stepId, WorkerProgressStepInfo info = null)
        {
        }

        public void StepFailed(string stepId, string message)
        {
        }

        public void ReportPhaseProgress(string label, int current, int total)
        {
        }

        public void ReportHeartbeat(string message)
        {
        }
    }
}
