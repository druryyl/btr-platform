using Microsoft.Extensions.Options;

namespace btr.infrastructure.Helpers
{
    public static class ConnStringHelper
    {
        private static ConnectionStringFactory _factory;

        public static string Server { get; private set; }
        public static string Database { get; private set; }

        public static void Initialize(ConnectionStringFactory factory)
        {
            _factory = factory;
        }

        internal static void SetResolvedValues(string server, string database)
        {
            Server = server;
            Database = database;
        }

        public static string Get(DatabaseOptions options)
        {
            if (_factory != null)
                return _factory.BuildConnectionString();

            return CreateFallbackFactory(options).BuildConnectionString();
        }

        public static IOptions<DatabaseOptions> GetTestOptions()
        {
            return Options.Create(new DatabaseOptions
            {
                ServerName = "JUDE7",
                DbName = "devTest",
                IsTest = true
            });
        }

        public static (string, string) ReadFromRegistry()
        {
            return RegistryConnectionSettingProvider.ReadFromRegistry();
        }

        private static ConnectionStringFactory CreateFallbackFactory(DatabaseOptions options)
        {
            IConnectionSettingProvider provider = options.IsTest
                ? (IConnectionSettingProvider)new JsonConnectionSettingProvider(Options.Create(options))
                : new RegistryConnectionSettingProvider(Options.Create(options));

            return new ConnectionStringFactory(provider);
        }
    }
}
