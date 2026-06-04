using System.Collections.Generic;
using System.Linq;
using System.Text;
using btr.application.SalesContext.SalesOmzetHealthWeeklyAgg.Policies;
using btr.domain.SalesContext.SalesOmzetHealthWeeklyAgg;

namespace btr.application.SalesContext.SalesOmzetHealthWeeklyAgg
{
    public static class SalesOmzetHealthLevelDisplay
    {
        public static string ToLabel(SalesOmzetHealthLevelEnum level)
        {
            switch (level)
            {
                case SalesOmzetHealthLevelEnum.Good:
                    return "Baik";
                case SalesOmzetHealthLevelEnum.Warning:
                    return "Peringatan";
                default:
                    return "Buruk";
            }
        }

        public static string FormatReportSummary(SalesOmzetReportHealthResult result)
        {
            if (result is null)
                return "Indikator tidak tersedia.";

            var avg = result.AverageScore.HasValue
                ? $" (rata-rata skor {result.AverageScore}, hanya info)"
                : string.Empty;

            return $"Status kesehatan: {ToLabel(result.FinalLevel)}{avg}";
        }

        public static string FormatWeekDetails(IReadOnlyList<SalesOmzetReportHealthWeekDetail> details)
        {
            if (details is null || details.Count == 0)
                return string.Empty;

            var lines = details.Select(d =>
            {
                if (!d.IsCalculated)
                    return $"Minggu {d.WeekNumber}/{d.YearNumber} → Belum dihitung (Buruk)";
                return $"Minggu {d.WeekNumber}/{d.YearNumber} → {ToLabel(d.Level)}";
            });

            return string.Join(" • ", lines);
        }
    }
}
