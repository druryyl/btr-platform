namespace btr.application.ReportingContext.DashboardSnapshotAgg.Progress
{
    public static class WorkerProgressStepIds
    {
        public const string LoadConfiguration = "config";
        public const string ValidateDatabase = "validate-db";
        public const string GenerateSummary = "summary";

        public static string DomainStep(string domain) => $"domain-{domain.ToLowerInvariant()}";
    }
}
