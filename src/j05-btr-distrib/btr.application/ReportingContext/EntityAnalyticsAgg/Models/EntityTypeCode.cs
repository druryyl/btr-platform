using System;
using System.Collections.Generic;

namespace btr.application.ReportingContext.EntityAnalyticsAgg.Models
{
    /// <summary>
    /// Well-known entity type codes. Registration metadata lives in <see cref="Contracts.IEntityTypeRegistry"/>.
    /// </summary>
    public static class EntityTypeCode
    {
        public const string Customer = "Customer";
        public const string Salesman = "Salesman";
        public const string Item = "Item";
        public const string Supplier = "Supplier";
        public const string Warehouse = "Warehouse";
        public const string Wilayah = "Wilayah";
        public const string Brand = "Brand";
        public const string Category = "Category";
        public const string Collector = "Collector";
    }
}
