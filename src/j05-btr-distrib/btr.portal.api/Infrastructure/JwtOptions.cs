namespace btr.portal.api.Infrastructure
{
    public class JwtOptions
    {
        public const string SectionName = "Jwt";

        public string Issuer { get; set; }
        public string Audience { get; set; }
        public string Key { get; set; }
        public int ExpiryMinutes { get; set; } = 480;
    }
}
