using System;
using System.Collections.Generic;
using btr.application.ReportingContext.EntityAnalyticsAgg.Models;

namespace btr.application.ReportingContext.EntityAnalyticsAgg.Backfill.Models
{
    public static class EntityAnalyticsBackfillExecutionOrder
    {
        public static readonly IReadOnlyList<string> EntityTypes = new[]
        {
            EntityTypeCode.Salesman,
            EntityTypeCode.Customer,
            EntityTypeCode.Supplier,
            EntityTypeCode.Item
        };

        public static readonly IReadOnlyDictionary<string, string> EntityTypeToWorkerDomain =
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                [EntityTypeCode.Customer] = "Customer",
                [EntityTypeCode.Salesman] = "Salesman",
                [EntityTypeCode.Supplier] = "PurchasingManagement",
                [EntityTypeCode.Item] = "InventoryRisk"
            };
    }
}
