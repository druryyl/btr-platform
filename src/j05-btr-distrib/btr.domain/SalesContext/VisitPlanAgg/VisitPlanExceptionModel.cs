using System;

namespace btr.domain.SalesContext.VisitPlanAgg
{
    public class VisitPlanExceptionModel : IVisitPlanExceptionKey
    {
        public string VisitPlanExceptionId { get; set; } = string.Empty;
        public string SalesPersonId { get; set; } = string.Empty;
        public DateTime VisitDate { get; set; }
        public string ExceptionType { get; set; } = string.Empty;
        public string CustomerId { get; set; } = string.Empty;
        public string ReplacementCustomerId { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public string CreatedByUserId { get; set; } = string.Empty;

        public string CustomerName { get; set; } = string.Empty;
        public string ReplacementCustomerName { get; set; } = string.Empty;
    }
}
