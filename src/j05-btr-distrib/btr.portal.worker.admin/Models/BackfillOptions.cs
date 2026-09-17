namespace btr.portal.worker.admin.Models
{
    public class BackfillOptions
    {
        public string EntityTypeScope { get; set; } = "All";
        public string Layers { get; set; } = "L1,L2,L5";
        public bool Resume { get; set; } = true;
        public bool Restart { get; set; }
        public bool Force { get; set; }
        public bool DryRun { get; set; }
        public bool ContinueOnError { get; set; }
        public int BatchSize { get; set; } = 500;
        public string ConfirmToken { get; set; }
        public bool SkipLiveMutexCheck { get; set; }
        public int? FromPeriodYear { get; set; }
        public int? FromPeriodMonth { get; set; }
        public int? ToPeriodYear { get; set; }
        public int? ToPeriodMonth { get; set; }
    }
}
