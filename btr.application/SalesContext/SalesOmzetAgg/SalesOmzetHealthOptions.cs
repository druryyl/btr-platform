using System;

namespace btr.application.SalesContext.SalesOmzetAgg
{
    public class SalesOmzetHealthOptions
    {
        public const string SectionName = "SalesOmzetHealth";

        /// <summary>Optional default end date for the 60-day health window (e.g. dev DB snapshot).</summary>
        public DateTime? WindowEndDate { get; set; }
    }
}
