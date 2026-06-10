namespace btr.domain.SalesContext.VisitPlanAgg
{
    public class EffectiveVisitPlanEntry
    {
        public string CustomerId { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public string CustomerCode { get; set; } = string.Empty;
        public int NoUrut { get; set; }
        public string Origin { get; set; } = "Template";
    }
}
