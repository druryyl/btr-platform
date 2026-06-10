using System;
using System.Collections.Generic;
using System.Linq;

namespace btr.portal.worker.Progress
{
    internal enum ConsoleTaskStatus
    {
        NotStarted,
        InProgress,
        Completed,
        Warning,
        Failed
    }

    internal sealed class ConsoleTaskTracker
    {
        private readonly List<string> _stepIds = new List<string>();
        private readonly Dictionary<string, string> _displayNames = new Dictionary<string, string>(StringComparer.Ordinal);
        private readonly Dictionary<string, ConsoleTaskStatus> _statuses = new Dictionary<string, ConsoleTaskStatus>(StringComparer.Ordinal);
        private readonly Dictionary<string, string> _completionDetails = new Dictionary<string, string>(StringComparer.Ordinal);
        private readonly object _sync = new object();

        public void BeginPlan(IReadOnlyList<string> stepIds, IReadOnlyList<string> displayNames)
        {
            lock (_sync)
            {
                _stepIds.Clear();
                _displayNames.Clear();
                _statuses.Clear();
                _completionDetails.Clear();

                for (var i = 0; i < stepIds.Count; i++)
                {
                    var id = stepIds[i];
                    var name = i < displayNames.Count ? displayNames[i] : id;
                    _stepIds.Add(id);
                    _displayNames[id] = name;
                    _statuses[id] = ConsoleTaskStatus.NotStarted;
                }
            }
        }

        public void SetStatus(string stepId, ConsoleTaskStatus status, string completionDetail = null)
        {
            lock (_sync)
            {
                if (!_displayNames.ContainsKey(stepId))
                    return;

                _statuses[stepId] = status;
                if (!string.IsNullOrWhiteSpace(completionDetail))
                    _completionDetails[stepId] = completionDetail;
            }
        }

        public IReadOnlyList<(string StepId, string DisplayName, ConsoleTaskStatus Status)> Snapshot()
        {
            lock (_sync)
            {
                return _stepIds
                    .Select(id => (id, _displayNames[id], _statuses.TryGetValue(id, out var status) ? status : ConsoleTaskStatus.NotStarted))
                    .ToList();
            }
        }

        public string GetDisplayName(string stepId)
        {
            lock (_sync)
            {
                return _displayNames.TryGetValue(stepId, out var name) ? name : stepId;
            }
        }

        public string GetCompletionDetail(string stepId)
        {
            lock (_sync)
            {
                return _completionDetails.TryGetValue(stepId, out var detail) ? detail : null;
            }
        }

        public void RenderPlanHeader(string title, string domain, string triggeredBy)
        {
            Console.Out.WriteLine(new string('=', 50));
            Console.Out.WriteLine(title);
            Console.Out.WriteLine(new string('=', 50));
            Console.Out.WriteLine($"Domain: {domain} | Triggered by: {triggeredBy}");
            Console.Out.WriteLine();
            Console.Out.WriteLine("This process will perform:");
            Console.Out.WriteLine();
            RenderChecklist();
            Console.Out.WriteLine();
            Console.Out.WriteLine("Please wait...");
            Console.Out.WriteLine();
        }

        public void RenderChecklist()
        {
            foreach (var item in Snapshot())
                WriteStatusLine(item.DisplayName, item.Status);
        }

        public void WriteStatusLine(string displayName, ConsoleTaskStatus status)
        {
            var marker = StatusMarker(status);
            var line = $"{marker} {displayName}";

            switch (status)
            {
                case ConsoleTaskStatus.Completed:
                    ConsoleColorSupport.WriteLine(ConsoleColor.Green, line);
                    break;
                case ConsoleTaskStatus.InProgress:
                    ConsoleColorSupport.WriteLine(ConsoleColor.Cyan, line);
                    break;
                case ConsoleTaskStatus.Warning:
                    ConsoleColorSupport.WriteLine(ConsoleColor.Yellow, line);
                    break;
                case ConsoleTaskStatus.Failed:
                    ConsoleColorSupport.WriteLine(ConsoleColor.Red, line);
                    break;
                default:
                    Console.Out.WriteLine(line);
                    break;
            }
        }

        public void WriteCompletionBlock(string displayName, string detail)
        {
            ConsoleColorSupport.WriteLine(ConsoleColor.Green, $"[x] {displayName} completed");
            if (!string.IsNullOrWhiteSpace(detail))
            {
                foreach (var line in detail.Split(new[] { Environment.NewLine }, StringSplitOptions.None))
                    Console.Out.WriteLine($"    {line}");
            }

            Console.Out.WriteLine();
        }

        public void RenderFinalSummary(
            IReadOnlyList<(string DisplayName, ConsoleTaskStatus Status, TimeSpan? Duration)> rows,
            TimeSpan totalDuration,
            bool failed)
        {
            Console.Out.WriteLine();
            Console.Out.WriteLine(new string('=', 50));
            ConsoleColorSupport.WriteLine(
                failed ? ConsoleColor.Red : ConsoleColor.Green,
                failed ? "PROCESS COMPLETED WITH ERRORS" : "PROCESS COMPLETED");
            Console.Out.WriteLine(new string('=', 50));
            Console.Out.WriteLine();

            foreach (var row in rows)
            {
                var statusText = RowStatusText(row.Status);
                var durationText = row.Duration.HasValue ? row.Duration.Value.ToString(@"hh\:mm\:ss") : string.Empty;
                var line = string.IsNullOrEmpty(durationText)
                    ? $"{row.DisplayName,-28}{statusText}"
                    : $"{row.DisplayName,-28}{statusText,-6}{durationText}";

                if (row.Status == ConsoleTaskStatus.Failed)
                    ConsoleColorSupport.WriteLine(ConsoleColor.Red, line);
                else if (row.Status == ConsoleTaskStatus.Warning)
                    ConsoleColorSupport.WriteLine(ConsoleColor.Yellow, line);
                else
                    Console.Out.WriteLine(line);
            }

            Console.Out.WriteLine();
            Console.Out.WriteLine($"Total Duration : {totalDuration:hh\\:mm\\:ss}");
            Console.Out.WriteLine();
        }

        private static string StatusMarker(ConsoleTaskStatus status)
        {
            switch (status)
            {
                case ConsoleTaskStatus.InProgress:
                    return "[>]";
                case ConsoleTaskStatus.Completed:
                    return "[x]";
                case ConsoleTaskStatus.Warning:
                    return "[!]";
                case ConsoleTaskStatus.Failed:
                    return "[X]";
                default:
                    return "[ ]";
            }
        }

        private static string RowStatusText(ConsoleTaskStatus status)
        {
            switch (status)
            {
                case ConsoleTaskStatus.Completed:
                    return "OK";
                case ConsoleTaskStatus.Warning:
                    return "WARN";
                case ConsoleTaskStatus.Failed:
                    return "FAIL";
                case ConsoleTaskStatus.InProgress:
                    return "RUN";
                default:
                    return "SKIP";
            }
        }
    }
}
