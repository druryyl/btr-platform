namespace btr.infrastructure.Helpers
{
    public class ConnectionStringFactory
    {
        private readonly IConnectionSettingProvider _provider;
        private string _connectionString = string.Empty;

        public ConnectionStringFactory(IConnectionSettingProvider provider)
        {
            _provider = provider;
        }

        public string BuildConnectionString()
        {
            if (_connectionString.Length > 0)
                return _connectionString;

            var server = _provider.GetServerName();
            var database = _provider.GetDatabaseName();

            ConnStringHelper.SetResolvedValues(server, database);

            const string uid = "btrLogin";
            const string pass = "btr123!";
            _connectionString =
                $"Server={server};Database={database};User Id={uid};Password={pass};TrustServerCertificate=True";

            return _connectionString;
        }
    }
}
