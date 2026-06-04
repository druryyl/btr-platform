using System;

namespace btr.application.SalesContext.SalesOmzetAgg
{
    /// <summary>Shared sentinel and void-date conventions for Sales Omzet aggregate.</summary>
    public static class SalesOmzetDates
    {
        public static readonly DateTime Sentinel = new DateTime(3000, 1, 1);

        public static bool IsSentinel(DateTime value) => value.Date == Sentinel.Date;

        public static bool IsNotVoid(DateTime voidDate) => voidDate.Date == Sentinel.Date;
    }
}
