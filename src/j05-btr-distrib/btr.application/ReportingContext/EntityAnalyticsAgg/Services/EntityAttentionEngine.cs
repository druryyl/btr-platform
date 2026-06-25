using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using btr.application.ReportingContext.EntityAnalyticsAgg.Backfill.Models;
using btr.application.ReportingContext.EntityAnalyticsAgg.Contracts;
using btr.application.ReportingContext.EntityAnalyticsAgg.Models;
using btr.application.ReportingContext.EntityAnalyticsAgg.Models.Snapshot;
using btr.application.ReportingContext.EntityAnalyticsAgg.Queries;

namespace btr.application.ReportingContext.EntityAnalyticsAgg.Services
{
    /// <summary>
    /// Generic L3 attention engine. Diffs producer signals against persisted lifecycle rows.
    /// Entity-agnostic — no domain-specific rules.
    /// </summary>
    public class EntityAttentionEngine : IEntityAttentionEngine
    {
        private readonly IEntityAnalyticsRepository _repository;

        public EntityAttentionEngine(IEntityAnalyticsRepository repository)
        {
            _repository = repository;
        }

        public void DiffAndPersistSignals(
            string entityType,
            int periodYear,
            int periodMonth,
            IReadOnlyDictionary<string, IReadOnlyList<EntityAttentionSignalSnapshot>> signalsByEntity,
            string refreshLogId,
            DateTime generatedAt,
            EntityAnalyticsReplayContext replay = null)
        {
            if (string.IsNullOrWhiteSpace(entityType) || signalsByEntity == null)
                return;

            if (replay == null && _repository.IsMonthClosed(entityType, periodYear, periodMonth))
                return;

            var updates = new List<EntityAnalyticsAttentionEventRow>();

            foreach (var pair in signalsByEntity)
            {
                var entityId = pair.Key;
                if (string.IsNullOrWhiteSpace(entityId))
                    continue;

                var currentSignals = (pair.Value ?? Array.Empty<EntityAttentionSignalSnapshot>())
                    .Where(s => !string.IsNullOrWhiteSpace(s?.SignalCode))
                    .GroupBy(s => s.SignalCode, StringComparer.OrdinalIgnoreCase)
                    .Select(g => g.First())
                    .ToDictionary(s => s.SignalCode, StringComparer.OrdinalIgnoreCase);

                var existingRows = _repository.GetAttentionEvents(entityType, entityId);
                var existingByCode = existingRows.ToDictionary(r => r.SignalCode, StringComparer.OrdinalIgnoreCase);

                foreach (var signal in currentSignals.Values)
                {
                    existingByCode.TryGetValue(signal.SignalCode, out var existing);
                    updates.Add(EntityAttentionLifecycleCalculator.ApplySignalPresent(
                        existing,
                        signal,
                        entityType,
                        periodYear,
                        periodMonth,
                        generatedAt));
                }

                foreach (var existing in existingRows)
                {
                    if (existing.IsActive && !currentSignals.ContainsKey(existing.SignalCode))
                    {
                        updates.Add(EntityAttentionLifecycleCalculator.ApplySignalAbsent(existing, generatedAt));
                    }
                }
            }

            if (replay != null)
                _repository.ReplaceAttentionForPeriod(entityType, periodYear, periodMonth, updates, refreshLogId);
            else
                _repository.SaveAttentionRecords(entityType, updates, refreshLogId);
        }

        public ProfileAttentionSectionDto BuildAttentionSection(string entityType, string entityId)
        {
            var rows = _repository.GetAttentionEvents(entityType, entityId);
            if (rows.Count == 0)
            {
                return new ProfileAttentionSectionDto
                {
                    IsAvailable = false,
                    UnavailableReason = EntityAnalyticsUnavailableReasons.NoSnapshotData,
                    ActiveSignalCount = 0,
                    HistoricalSignalCount = 0,
                    Events = new List<ProfileAttentionEventDto>()
                };
            }

            var events = rows
                .OrderByDescending(r => r.IsActive)
                .ThenByDescending(r => r.LastSeenPeriodYear)
                .ThenByDescending(r => r.LastSeenPeriodMonth)
                .ThenBy(r => r.SignalTitle, StringComparer.OrdinalIgnoreCase)
                .Select(MapEvent)
                .ToList();

            return new ProfileAttentionSectionDto
            {
                IsAvailable = true,
                ActiveSignalCount = events.Count(e => e.IsActive),
                HistoricalSignalCount = events.Count(e => !e.IsActive),
                Events = events
            };
        }

        private static ProfileAttentionEventDto MapEvent(EntityAnalyticsAttentionEventRow row)
        {
            return new ProfileAttentionEventDto
            {
                SignalCode = row.SignalCode,
                SignalLabel = row.SignalTitle,
                SignalCategory = row.SignalCategory,
                IsActive = row.IsActive,
                FirstSeenPeriodYear = row.FirstSeenPeriodYear,
                FirstSeenPeriodMonth = row.FirstSeenPeriodMonth,
                LastSeenPeriodYear = row.LastSeenPeriodYear,
                LastSeenPeriodMonth = row.LastSeenPeriodMonth,
                FirstSeen = FormatPeriod(row.FirstSeenPeriodYear, row.FirstSeenPeriodMonth),
                LastSeen = FormatPeriod(row.LastSeenPeriodYear, row.LastSeenPeriodMonth),
                ConsecutivePeriods = row.ConsecutivePeriods,
                TotalOccurrences = row.TotalOccurrences
            };
        }

        private static string FormatPeriod(int year, int month)
        {
            if (year <= 0 || month <= 0 || month > 12)
                return null;

            var monthName = CultureInfo.InvariantCulture.DateTimeFormat.GetAbbreviatedMonthName(month);
            return $"{monthName} {year}";
        }
    }
}
