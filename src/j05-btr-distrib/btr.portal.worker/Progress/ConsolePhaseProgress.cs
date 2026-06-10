using System;
using System.Text;

namespace btr.portal.worker.Progress
{
    internal static class ConsolePhaseProgress
    {
        private const int BarWidth = 20;

        public static string Format(string label, int current, int total)
        {
            if (total <= 0)
                return label;

            var percent = (int)Math.Round(current * 100.0 / total);
            var filled = (int)Math.Round(BarWidth * current / (double)total);
            filled = Math.Max(0, Math.Min(BarWidth, filled));

            var bar = new StringBuilder(BarWidth);
            for (var i = 0; i < BarWidth; i++)
                bar.Append(i < filled ? '#' : '-');

            return $"{label}{Environment.NewLine}[{bar}] {percent}%{Environment.NewLine}{current:N0} / {total:N0}";
        }

        public static string FormatWithEta(string label, int current, int total, TimeSpan elapsed)
        {
            var baseText = Format(label, current, total);
            if (current <= 0 || current >= total)
                return baseText;

            var remaining = TimeSpan.FromTicks(elapsed.Ticks * (total - current) / current);
            return $"{baseText}{Environment.NewLine}Estimated remaining: {remaining:hh\\:mm\\:ss}";
        }
    }
}
