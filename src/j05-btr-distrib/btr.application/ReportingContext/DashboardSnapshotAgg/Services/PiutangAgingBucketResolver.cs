using System;

namespace btr.application.ReportingContext.DashboardSnapshotAgg.Services
{
    public static class PiutangAgingBucketResolver
    {
        public static readonly (string Key, string Label, int SortOrder)[] BucketDefinitions =
        {
            ("Current", "Current (Not Yet Due)", 1),
            ("Days1To30", "1–30 Days", 2),
            ("Days31To60", "31–60 Days", 3),
            ("Days61To90", "61–90 Days", 4),
            ("DaysOver90", "> 90 Days", 5),
        };

        public static string ResolveBucketKey(DateTime jatuhTempo, DateTime today)
        {
            var daysOverdue = ResolveDaysOverdue(jatuhTempo, today);

            if (daysOverdue <= 0) return "Current";
            if (daysOverdue <= 30) return "Days1To30";
            if (daysOverdue <= 60) return "Days31To60";
            if (daysOverdue <= 90) return "Days61To90";
            return "DaysOver90";
        }

        public static int ResolveDaysOverdue(DateTime jatuhTempo, DateTime today)
        {
            return (today - jatuhTempo.Date).Days;
        }
    }
}
