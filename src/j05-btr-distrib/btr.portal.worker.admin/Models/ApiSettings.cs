namespace btr.portal.worker.admin.Models
{
    public class ApiSettings
    {
        public string ApiBaseUrl { get; set; }
        public string WorkerExePath { get; set; }
        public int HealthPollIntervalSeconds { get; set; }
        public int ApiTimeoutSeconds { get; set; }
        public string RefreshToken { get; set; }
    }
}
