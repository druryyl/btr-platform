using System;
using System.Globalization;
using btr.application.ReportingContext.EntityAnalyticsAgg.Models;
using btr.application.ReportingContext.EntityAnalyticsAgg.Models.Snapshot;
using btr.application.ReportingContext.EntityAnalyticsAgg.Queries;

namespace btr.application.ReportingContext.EntityAnalyticsAgg.Services
{
    /// <summary>Formats KPI snapshot values into presentation envelopes using registry metadata.</summary>
    public class EntityKpiEnvelopeFormatter
    {
        public KpiEnvelopeDto Map(EntityAnalyticsCurrentRow row, EntityKpiMetadata metadata)
        {
            if (row is null)
                throw new ArgumentNullException(nameof(row));
            if (metadata is null)
                throw new ArgumentNullException(nameof(metadata));

            return new KpiEnvelopeDto
            {
                KpiId = row.KpiId,
                Category = metadata.Category.ToString(),
                DisplayName = metadata.DisplayName,
                Value = row.NumericValue,
                TextValue = row.TextValue,
                FormattedValue = FormatValue(row.NumericValue, row.TextValue, metadata),
                Unit = metadata.Unit,
                Direction = metadata.Direction,
                PeriodLabel = metadata.PeriodSemantics,
                EvidenceRoute = metadata.EvidenceRoute,
                FilterDimension = metadata.EvidenceFilterDimension,
                ValueType = metadata.ValueType,
                DisplayPrecision = metadata.DisplayPrecision,
                TrendEligible = metadata.TrendEligible,
                RankEligible = metadata.RankEligible,
                NullableBehavior = metadata.NullableBehavior
            };
        }

        public KpiEnvelopeDto MapFromMonthly(EntityAnalyticsMonthlyRow row, EntityKpiMetadata metadata)
        {
            if (row is null)
                throw new ArgumentNullException(nameof(row));
            if (metadata is null)
                throw new ArgumentNullException(nameof(metadata));

            return new KpiEnvelopeDto
            {
                KpiId = row.KpiId,
                Category = metadata.Category.ToString(),
                DisplayName = metadata.DisplayName,
                Value = row.NumericValue,
                TextValue = row.TextValue,
                FormattedValue = FormatValue(row.NumericValue, row.TextValue, metadata),
                Unit = metadata.Unit,
                Direction = metadata.Direction,
                PeriodLabel = row.PeriodSemantics ?? metadata.PeriodSemantics,
                EvidenceRoute = metadata.EvidenceRoute,
                FilterDimension = metadata.EvidenceFilterDimension,
                ValueType = metadata.ValueType,
                DisplayPrecision = metadata.DisplayPrecision,
                TrendEligible = metadata.TrendEligible,
                RankEligible = metadata.RankEligible,
                NullableBehavior = metadata.NullableBehavior
            };
        }

        public string FormatValue(EntityAnalyticsCurrentRow row, EntityKpiMetadata metadata)
        {
            return FormatValue(row.NumericValue, row.TextValue, metadata);
        }

        public string FormatValue(decimal? numericValue, string textValue, EntityKpiMetadata metadata)
        {
            if (!string.IsNullOrWhiteSpace(textValue))
                return textValue;

            if (!numericValue.HasValue)
                return FormatNullable(metadata.NullableBehavior);

            var precision = metadata.DisplayPrecision >= 0
                ? metadata.DisplayPrecision
                : ResolveDefaultPrecision(metadata.Unit);

            if (string.Equals(metadata.Unit, "IDR", StringComparison.OrdinalIgnoreCase))
                return numericValue.Value.ToString("N" + precision, CultureInfo.InvariantCulture);

            if (string.Equals(metadata.Unit, "Percent", StringComparison.OrdinalIgnoreCase))
                return numericValue.Value.ToString("N" + precision, CultureInfo.InvariantCulture) + "%";

            return numericValue.Value.ToString("N" + precision, CultureInfo.InvariantCulture);
        }

        private static int ResolveDefaultPrecision(string unit)
        {
            if (string.Equals(unit, "IDR", StringComparison.OrdinalIgnoreCase)
                || string.Equals(unit, "Count", StringComparison.OrdinalIgnoreCase))
            {
                return 0;
            }

            return 2;
        }

        private static string FormatNullable(string nullableBehavior)
        {
            if (string.Equals(nullableBehavior, "ShowDash", StringComparison.OrdinalIgnoreCase))
                return "—";

            return string.Empty;
        }
    }
}
