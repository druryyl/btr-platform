using System.Collections.Generic;

namespace btr.application.ReportingContext.EntityAnalyticsAgg.Models
{
    public class EntityIdentity
    {
        public string EntityType { get; set; }

        public string EntityId { get; set; }

        public string EntityCode { get; set; }

        public string DisplayName { get; set; }

        public bool IsActive { get; set; }

        public IReadOnlyDictionary<string, string> Dimensions { get; set; }
            = new Dictionary<string, string>();
    }
}
