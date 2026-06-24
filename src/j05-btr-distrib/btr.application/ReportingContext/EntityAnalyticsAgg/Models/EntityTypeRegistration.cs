namespace btr.application.ReportingContext.EntityAnalyticsAgg.Models
{
    public class EntityTypeRegistration
    {
        public string EntityTypeCode { get; set; }

        public string DisplayName { get; set; }

        public string MasterDalContract { get; set; }

        public string EntityIdResolver { get; set; }

        public string KpiPackId { get; set; }

        public string RelationshipPackId { get; set; }

        public string PeerGroupRuleId { get; set; }

        public string ProducerType { get; set; }

        public string WorkerDomainHook { get; set; }

        public string ProfileRouteTemplate { get; set; }
    }
}
