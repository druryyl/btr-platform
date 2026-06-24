using System;
using System.Collections.Generic;

namespace btr.application.ReportingContext.EntityAnalyticsAgg.Models
{
    public class EntityOverview
    {
        public EntityIdentity Identity { get; set; }

        public DateTime? GeneratedAt { get; set; }

        public IReadOnlyDictionary<string, string> StatusDimensions { get; set; }
            = new Dictionary<string, string>();
    }
}
