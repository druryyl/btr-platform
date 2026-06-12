namespace btr.application.Portal
{
    public sealed class PresentationOptions
    {
        public const string SECTION_NAME = "Presentation";

        public bool Enabled { get; set; }

        /// <summary>ISO date (yyyy-MM-dd) used as business "today" when <see cref="Enabled"/> is true.</summary>
        public string BusinessDate { get; set; }
    }
}
