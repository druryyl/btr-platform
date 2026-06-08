using System;

namespace btr.application.ReportingContext.PiutangReportAgg
{
    public enum PiutangReportDateField
    {
        DueDate,
        PiutangDate,
    }

    public static class PiutangReportDateFieldParser
    {
        public static PiutangReportDateField Parse(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return PiutangReportDateField.DueDate;

            if (Enum.TryParse(value, ignoreCase: true, result: out PiutangReportDateField parsed))
                return parsed;

            throw new ArgumentException("dateField must be 'DueDate' or 'PiutangDate'.");
        }
    }
}
