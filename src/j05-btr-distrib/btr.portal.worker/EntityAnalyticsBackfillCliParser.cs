using System;
using System.Globalization;
using btr.application.ReportingContext.EntityAnalyticsAgg.Backfill.Models;

namespace btr.portal.worker
{
    internal static class EntityAnalyticsBackfillCliParser
    {
        public static EntityAnalyticsBackfillRequest Parse(string[] args, string triggeredBy)
        {
            var request = new EntityAnalyticsBackfillRequest
            {
                TriggeredBy = triggeredBy,
                EntityTypeScope = ParseRequiredOption(args, "--entity-type", "All"),
                Layers = ParseRequiredOption(args, "--layers", "L1,L2,L5"),
                Resume = ParseOptionalBool(args, "--resume", true),
                Restart = ParseFlag(args, "--restart"),
                Force = ParseFlag(args, "--force"),
                DryRun = ParseFlag(args, "--dry-run"),
                ContinueOnError = ParseFlag(args, "--continue-on-error"),
                BatchSize = ParseOptionalInt(args, "--batch-size", 500),
                ConfirmToken = ParseOptionalOption(args, "--confirm"),
                SkipLiveMutexCheck = ParseFlag(args, "--skip-live-mutex-check")
            };

            var fromPeriod = ParseOptionalOption(args, "--from-period");
            if (!string.IsNullOrWhiteSpace(fromPeriod))
            {
                var parsedFrom = YearMonthPeriod.Parse(fromPeriod);
                request.FromPeriodYear = parsedFrom.Year;
                request.FromPeriodMonth = parsedFrom.Month;
            }

            var toPeriod = ParseOptionalOption(args, "--to-period");
            if (!string.IsNullOrWhiteSpace(toPeriod))
            {
                var parsedTo = YearMonthPeriod.Parse(toPeriod);
                request.ToPeriodYear = parsedTo.Year;
                request.ToPeriodMonth = parsedTo.Month;
            }

            if (request.Restart)
                request.Resume = false;

            return request;
        }

        private static string ParseRequiredOption(string[] args, string name, string defaultValue)
        {
            for (var i = 0; i < args.Length; i++)
            {
                if (!string.Equals(args[i], name, StringComparison.OrdinalIgnoreCase))
                    continue;

                if (i + 1 >= args.Length || args[i + 1].StartsWith("--", StringComparison.Ordinal))
                    throw new ArgumentException($"Missing value for {name}.");

                return args[i + 1];
            }

            return defaultValue;
        }

        private static string ParseOptionalOption(string[] args, string name)
        {
            for (var i = 0; i < args.Length; i++)
            {
                if (!string.Equals(args[i], name, StringComparison.OrdinalIgnoreCase))
                    continue;

                if (i + 1 >= args.Length || args[i + 1].StartsWith("--", StringComparison.Ordinal))
                    throw new ArgumentException($"Missing value for {name}.");

                return args[i + 1];
            }

            return null;
        }

        private static bool ParseFlag(string[] args, string name)
        {
            for (var i = 0; i < args.Length; i++)
            {
                if (string.Equals(args[i], name, StringComparison.OrdinalIgnoreCase))
                    return true;
            }

            return false;
        }

        private static bool ParseOptionalBool(string[] args, string name, bool defaultValue)
        {
            var value = ParseOptionalOption(args, name);
            if (string.IsNullOrWhiteSpace(value))
                return defaultValue;

            if (bool.TryParse(value, out var parsed))
                return parsed;

            if (string.Equals(value, "1", StringComparison.OrdinalIgnoreCase))
                return true;

            if (string.Equals(value, "0", StringComparison.OrdinalIgnoreCase))
                return false;

            throw new ArgumentException($"Invalid boolean value '{value}' for {name}.");
        }

        private static int ParseOptionalInt(string[] args, string name, int defaultValue)
        {
            var value = ParseOptionalOption(args, name);
            if (string.IsNullOrWhiteSpace(value))
                return defaultValue;

            if (int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed))
                return parsed;

            throw new ArgumentException($"Invalid integer value '{value}' for {name}.");
        }
    }
}
