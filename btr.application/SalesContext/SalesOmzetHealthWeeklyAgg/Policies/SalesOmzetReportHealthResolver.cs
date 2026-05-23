using System;
using System.Collections.Generic;
using System.Linq;
using btr.domain.SalesContext.SalesOmzetHealthWeeklyAgg;
using btr.nuna.Domain;

namespace btr.application.SalesContext.SalesOmzetHealthWeeklyAgg.Policies
{
    public class SalesOmzetReportHealthResolver : ISalesOmzetReportHealthResolver
    {
        public SalesOmzetReportHealthResult Resolve(
            Periode reportPeriod,
            IReadOnlyList<IsoWeekIdentifier> intersectingWeeks,
            IReadOnlyList<SalesOmzetHealthWeeklyModel> persistedRows)
        {
            if (reportPeriod is null)
                throw new ArgumentNullException(nameof(reportPeriod));
            if (intersectingWeeks is null)
                throw new ArgumentNullException(nameof(intersectingWeeks));
            if (persistedRows is null)
                throw new ArgumentNullException(nameof(persistedRows));

            var byWeek = persistedRows.ToDictionary(
                r => (r.YearNumber, r.WeekNumber),
                r => r);

            var details = new List<SalesOmzetReportHealthWeekDetail>();
            var levels = new List<SalesOmzetHealthLevelEnum>();
            var scores = new List<int>();

            foreach (var week in intersectingWeeks)
            {
                if (byWeek.TryGetValue((week.YearNumber, week.WeekNumber), out var row))
                {
                    details.Add(new SalesOmzetReportHealthWeekDetail
                    {
                        YearNumber = week.YearNumber,
                        WeekNumber = week.WeekNumber,
                        Level = row.HealthLevel,
                        HealthScore = row.HealthScore,
                        IsCalculated = true
                    });
                    levels.Add(row.HealthLevel);
                    scores.Add(row.HealthScore);
                }
                else
                {
                    details.Add(new SalesOmzetReportHealthWeekDetail
                    {
                        YearNumber = week.YearNumber,
                        WeekNumber = week.WeekNumber,
                        Level = SalesOmzetHealthLevelEnum.Poor,
                        HealthScore = null,
                        IsCalculated = false
                    });
                    levels.Add(SalesOmzetHealthLevelEnum.Poor);
                }
            }

            return new SalesOmzetReportHealthResult
            {
                FinalLevel = ResolveWorstBucket(levels),
                AverageScore = scores.Count > 0 ? (int?)Math.Round(scores.Average()) : null,
                WeekDetails = details
            };
        }

        private static SalesOmzetHealthLevelEnum ResolveWorstBucket(IEnumerable<SalesOmzetHealthLevelEnum> levels)
        {
            if (levels.Any(l => l == SalesOmzetHealthLevelEnum.Poor))
                return SalesOmzetHealthLevelEnum.Poor;
            if (levels.Any(l => l == SalesOmzetHealthLevelEnum.Warning))
                return SalesOmzetHealthLevelEnum.Warning;
            return SalesOmzetHealthLevelEnum.Good;
        }
    }
}
