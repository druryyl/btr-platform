using Microsoft.Extensions.Options;

namespace btr.infrastructure.Helpers
{
    public class JsonConnectionSettingProvider : IConnectionSettingProvider
    {
        private readonly DatabaseOptions _options;

        public JsonConnectionSettingProvider(IOptions<DatabaseOptions> options)
        {
            _options = options.Value;
        }

        public string GetServerName()
        {
            return _options.ServerName ?? string.Empty;
        }

        public string GetDatabaseName()
        {
            return _options.DbName ?? string.Empty;
        }
    }
}
