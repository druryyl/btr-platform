using System;

namespace btr.domain.SalesContext.VisitPlanAgg
{
    public class VisitPlanModel : IVisitPlanKey
    {
        public string VisitPlanId { get; set; } = string.Empty;
        public string SalesPersonId { get; set; } = string.Empty;
        public DateTime VisitDate { get; set; }
        public string CustomerId { get; set; } = string.Empty;
        public int NoUrut { get; set; }
        public string HariRuteId { get; set; } = string.Empty;
        public string PlanSource { get; set; } = "Template";
        public DateTime MaterializedAt { get; set; }

        public string CustomerName { get; set; } = string.Empty;
        public string CustomerCode { get; set; } = string.Empty;
    }
}
