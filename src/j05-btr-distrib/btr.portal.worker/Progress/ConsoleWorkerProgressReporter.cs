using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using btr.application.ReportingContext.DashboardSnapshotAgg.Progress;
using WorkerStepIds = btr.application.ReportingContext.DashboardSnapshotAgg.Progress.WorkerProgressStepIds;

namespace btr.portal.worker.Progress
{
    internal sealed class ConsoleWorkerProgressReporter : IWorkerProgressReporter, IDisposable
    {
        private static readonly TimeSpan HeartbeatInterval = TimeSpan.FromSeconds(30);

        private readonly ConsoleTaskTracker _tracker = new ConsoleTaskTracker();
        private readonly object _sync = new object();
        private ConsoleHeartbeatTimer _heartbeat;
        private string _activeStepId;
        private string _activeSubStepDisplayName;
        private string _activePhaseLabel;
        private int _phaseCurrent;
        private int _phaseTotal;
        private DateTime _phaseStartedAt;

        public ConsoleTaskTracker Tracker => _tracker;

        public void BeginPlan(IReadOnlyList<string> stepIds, IReadOnlyList<string> displayNames)
        {
            lock (_sync)
            {
                _tracker.BeginPlan(stepIds, displayNames);
            }
        }

        public void RenderStartupHeader(string title, string domain, string triggeredBy)
        {
            _tracker.RenderPlanHeader(title, domain, triggeredBy);
        }

        public void StepStarted(string stepId, string displayName)
        {
            lock (_sync)
            {
                StopHeartbeat();

                if (IsTopLevelStep(stepId))
                {
                    _tracker.SetStatus(stepId, ConsoleTaskStatus.InProgress);
                    _tracker.WriteStatusLine(displayName, ConsoleTaskStatus.InProgress);
                    _activeStepId = stepId;
                    StartHeartbeat(displayName);
                    return;
                }

                Console.Out.WriteLine($"    [>] {displayName}");
                _activeSubStepDisplayName = displayName;
                _activePhaseLabel = displayName;
                _phaseCurrent = 0;
                _phaseTotal = 0;
                _phaseStartedAt = DateTime.UtcNow;
                StartHeartbeat(displayName);
            }
        }

        public void StepCompleted(string stepId, WorkerProgressStepInfo info = null)
        {
            lock (_sync)
            {
                StopHeartbeat();

                if (IsTopLevelStep(stepId))
                {
                    var displayName = _tracker.GetDisplayName(stepId);
                    var detail = BuildCompletionDetail(info);
                    _tracker.SetStatus(stepId, ConsoleTaskStatus.Completed, detail);
                    _tracker.WriteCompletionBlock(displayName, detail);
                    _activeStepId = null;
                    return;
                }

                var subDetail = BuildSubStepDetail(info);
                var subDisplayName = _activeSubStepDisplayName ?? stepId;
                ConsoleColorSupport.WriteLine(ConsoleColor.Green, $"    [x] {subDisplayName} completed");
                _activeSubStepDisplayName = null;
                if (!string.IsNullOrWhiteSpace(subDetail))
                {
                    foreach (var line in subDetail.Split(new[] { Environment.NewLine }, StringSplitOptions.None))
                        Console.Out.WriteLine($"        {line}");
                }
            }
        }

        public void StepFailed(string stepId, string message)
        {
            lock (_sync)
            {
                StopHeartbeat();

                if (IsTopLevelStep(stepId))
                {
                    var displayName = _tracker.GetDisplayName(stepId);
                    _tracker.SetStatus(stepId, ConsoleTaskStatus.Failed, message);
                    ConsoleColorSupport.WriteLine(ConsoleColor.Red, $"[X] {displayName} failed");
                    if (!string.IsNullOrWhiteSpace(message))
                        Console.Out.WriteLine($"    {message}");
                    Console.Out.WriteLine();
                    _activeStepId = null;
                    return;
                }

                ConsoleColorSupport.WriteLine(ConsoleColor.Red, $"    [X] {stepId} failed");
                if (!string.IsNullOrWhiteSpace(message))
                    Console.Out.WriteLine($"        {message}");
            }
        }

        public void ReportPhaseProgress(string label, int current, int total)
        {
            lock (_sync)
            {
                _activePhaseLabel = label;
                _phaseCurrent = current;
                _phaseTotal = total;
                if (_phaseStartedAt == default)
                    _phaseStartedAt = DateTime.UtcNow;

                var elapsed = DateTime.UtcNow - _phaseStartedAt;
                var text = total > 0
                    ? ConsolePhaseProgress.FormatWithEta($"    {label}", current, total, elapsed)
                    : $"    {label}";

                Console.Out.WriteLine(text);
                Console.Out.WriteLine();
            }
        }

        public void ReportHeartbeat(string message)
        {
            lock (_sync)
            {
                if (!string.IsNullOrWhiteSpace(message))
                    ConsoleColorSupport.WriteLine(ConsoleColor.DarkCyan, message);
            }
        }

        public IReadOnlyList<(string DisplayName, ConsoleTaskStatus Status, TimeSpan? Duration)> BuildSummaryRows(
            IReadOnlyDictionary<string, TimeSpan> durations)
        {
            var rows = new List<(string, ConsoleTaskStatus, TimeSpan?)>();
            foreach (var item in _tracker.Snapshot())
            {
                durations.TryGetValue(item.StepId, out var duration);
                rows.Add((item.DisplayName, item.Status, duration == default ? (TimeSpan?)null : duration));
            }

            return rows;
        }

        public void Dispose()
        {
            StopHeartbeat();
        }

        private void StartHeartbeat(string contextLabel)
        {
            _heartbeat = new ConsoleHeartbeatTimer(HeartbeatInterval, () =>
            {
                var elapsed = _heartbeat?.Elapsed ?? TimeSpan.Zero;
                if (_phaseTotal > 0 && _phaseCurrent > 0 && !string.IsNullOrWhiteSpace(_activePhaseLabel))
                {
                    return $"Still processing {contextLabel}...{Environment.NewLine}" +
                           ConsolePhaseProgress.FormatWithEta($"    {_activePhaseLabel}", _phaseCurrent, _phaseTotal, elapsed);
                }

                return $"Still processing {contextLabel}...{Environment.NewLine}Elapsed: {elapsed:hh\\:mm\\:ss}";
            });
        }

        private void StopHeartbeat()
        {
            _heartbeat?.Dispose();
            _heartbeat = null;
            _activePhaseLabel = null;
            _phaseCurrent = 0;
            _phaseTotal = 0;
            _phaseStartedAt = default;
        }

        private static bool IsTopLevelStep(string stepId)
        {
            if (string.IsNullOrWhiteSpace(stepId))
                return false;

            return stepId == WorkerStepIds.LoadConfiguration
                   || stepId == WorkerStepIds.ValidateDatabase
                   || stepId == WorkerStepIds.GenerateSummary
                   || stepId.StartsWith("domain-", StringComparison.OrdinalIgnoreCase);
        }

        private static string BuildCompletionDetail(WorkerProgressStepInfo info)
        {
            if (info == null)
                return null;

            var builder = new StringBuilder();
            if (info.RecordCount.HasValue)
                builder.AppendLine($"Records loaded: {info.RecordCount.Value.ToString("N0", CultureInfo.CurrentCulture)}");

            if (!string.IsNullOrWhiteSpace(info.Detail))
                builder.AppendLine(info.Detail);

            if (info.Duration.HasValue)
                builder.Append($"Duration: {info.Duration.Value:hh\\:mm\\:ss}");

            return builder.Length == 0 ? null : builder.ToString().TrimEnd();
        }

        private static string BuildSubStepDetail(WorkerProgressStepInfo info)
        {
            if (info == null)
                return null;

            var builder = new StringBuilder();
            if (info.RecordCount.HasValue)
                builder.AppendLine($"Records: {info.RecordCount.Value.ToString("N0", CultureInfo.CurrentCulture)}");

            if (!string.IsNullOrWhiteSpace(info.Detail))
                builder.AppendLine(info.Detail);

            if (info.Duration.HasValue)
                builder.Append($"Duration: {info.Duration.Value:hh\\:mm\\:ss}");

            return builder.Length == 0 ? null : builder.ToString().TrimEnd();
        }
    }

    internal static class WorkerDomainOrder
    {
        public static readonly string[] AllDomainOrder =
        {
            "Piutang",
            "Inventory",
            "InventoryRisk",
            "Sales",
            "Purchasing",
            "PurchasingManagement",
            "Customer",
            "Salesman",
            "Collection",
            "FieldActivity",
            "Location"
        };
    }
}
