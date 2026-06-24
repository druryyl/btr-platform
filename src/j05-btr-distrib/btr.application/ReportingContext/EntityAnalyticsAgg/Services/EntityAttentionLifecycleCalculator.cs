using System;
using btr.application.ReportingContext.EntityAnalyticsAgg.Models;
using btr.application.ReportingContext.EntityAnalyticsAgg.Models.Snapshot;

namespace btr.application.ReportingContext.EntityAnalyticsAgg.Services
{
    /// <summary>Pure lifecycle rules for L3 attention signal diffing.</summary>
    public static class EntityAttentionLifecycleCalculator
    {
        public static EntityAnalyticsAttentionEventRow ApplySignalPresent(
            EntityAnalyticsAttentionEventRow existing,
            EntityAttentionSignalSnapshot signal,
            string entityType,
            int periodYear,
            int periodMonth,
            DateTime generatedAt)
        {
            if (signal == null)
                throw new ArgumentNullException(nameof(signal));

            if (existing == null)
            {
                return CreateNew(signal, entityType, periodYear, periodMonth, generatedAt);
            }

            if (!existing.IsActive)
            {
                return Reactivate(existing, signal, periodYear, periodMonth, generatedAt);
            }

            if (existing.LastSeenPeriodYear == periodYear && existing.LastSeenPeriodMonth == periodMonth)
            {
                return Touch(existing, signal, generatedAt);
            }

            if (IsNextConsecutiveMonth(
                existing.LastSeenPeriodYear,
                existing.LastSeenPeriodMonth,
                periodYear,
                periodMonth))
            {
                existing.ConsecutivePeriods += 1;
            }
            else
            {
                existing.ConsecutivePeriods = 1;
            }

            existing.TotalOccurrences += 1;
            existing.LastSeenPeriodYear = periodYear;
            existing.LastSeenPeriodMonth = periodMonth;
            existing.SignalCategory = signal.SignalCategory ?? existing.SignalCategory;
            existing.SignalTitle = signal.SignalTitle ?? existing.SignalTitle;
            existing.EntityCode = signal.EntityCode ?? existing.EntityCode;
            existing.IsActive = true;
            existing.GeneratedAt = generatedAt;
            existing.UpdatedAt = generatedAt;
            return existing;
        }

        public static EntityAnalyticsAttentionEventRow ApplySignalAbsent(
            EntityAnalyticsAttentionEventRow existing,
            DateTime generatedAt)
        {
            if (existing == null || !existing.IsActive)
                return existing;

            existing.IsActive = false;
            existing.GeneratedAt = generatedAt;
            existing.UpdatedAt = generatedAt;
            return existing;
        }

        private static EntityAnalyticsAttentionEventRow CreateNew(
            EntityAttentionSignalSnapshot signal,
            string entityType,
            int periodYear,
            int periodMonth,
            DateTime generatedAt)
        {
            return new EntityAnalyticsAttentionEventRow
            {
                EntityType = entityType,
                EntityId = signal.EntityId,
                EntityCode = signal.EntityCode ?? signal.EntityId,
                SignalCode = signal.SignalCode,
                SignalCategory = signal.SignalCategory ?? "General",
                SignalTitle = signal.SignalTitle ?? signal.SignalCode,
                FirstSeenPeriodYear = periodYear,
                FirstSeenPeriodMonth = periodMonth,
                LastSeenPeriodYear = periodYear,
                LastSeenPeriodMonth = periodMonth,
                ConsecutivePeriods = 1,
                TotalOccurrences = 1,
                IsActive = true,
                GeneratedAt = generatedAt,
                CreatedAt = generatedAt,
                UpdatedAt = generatedAt
            };
        }

        private static EntityAnalyticsAttentionEventRow Reactivate(
            EntityAnalyticsAttentionEventRow existing,
            EntityAttentionSignalSnapshot signal,
            int periodYear,
            int periodMonth,
            DateTime generatedAt)
        {
            existing.IsActive = true;
            existing.ConsecutivePeriods = 1;
            existing.TotalOccurrences += 1;
            existing.LastSeenPeriodYear = periodYear;
            existing.LastSeenPeriodMonth = periodMonth;
            existing.SignalCategory = signal.SignalCategory ?? existing.SignalCategory;
            existing.SignalTitle = signal.SignalTitle ?? existing.SignalTitle;
            existing.EntityCode = signal.EntityCode ?? existing.EntityCode;
            existing.GeneratedAt = generatedAt;
            existing.UpdatedAt = generatedAt;
            return existing;
        }

        private static EntityAnalyticsAttentionEventRow Touch(
            EntityAnalyticsAttentionEventRow existing,
            EntityAttentionSignalSnapshot signal,
            DateTime generatedAt)
        {
            existing.SignalCategory = signal.SignalCategory ?? existing.SignalCategory;
            existing.SignalTitle = signal.SignalTitle ?? existing.SignalTitle;
            existing.EntityCode = signal.EntityCode ?? existing.EntityCode;
            existing.GeneratedAt = generatedAt;
            existing.UpdatedAt = generatedAt;
            return existing;
        }

        private static bool IsNextConsecutiveMonth(
            int lastYear,
            int lastMonth,
            int currentYear,
            int currentMonth)
        {
            if (lastYear == currentYear && lastMonth == currentMonth)
                return false;

            var last = new DateTime(lastYear, lastMonth, 1);
            var next = last.AddMonths(1);
            return next.Year == currentYear && next.Month == currentMonth;
        }
    }
}
