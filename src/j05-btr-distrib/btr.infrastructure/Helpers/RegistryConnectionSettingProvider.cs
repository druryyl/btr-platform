using Microsoft.Extensions.Options;
using Microsoft.Win32;

namespace btr.infrastructure.Helpers
{
    public class RegistryConnectionSettingProvider : IConnectionSettingProvider
    {
        private readonly DatabaseOptions _options;
        private string _resolvedServer;
        private string _resolvedDatabase;
        private bool _resolved;

        public RegistryConnectionSettingProvider(IOptions<DatabaseOptions> options)
        {
            _options = options.Value;
        }

        public string GetServerName()
        {
            EnsureResolved();
            return _resolvedServer;
        }

        public string GetDatabaseName()
        {
            EnsureResolved();
            return _resolvedDatabase;
        }

        private void EnsureResolved()
        {
            if (_resolved)
                return;

            if (_options.IsTest)
            {
                _resolvedServer = _options.ServerName ?? string.Empty;
                _resolvedDatabase = _options.DbName ?? string.Empty;
            }
            else
            {
                var (regServer, regDb) = ReadFromRegistry();
                _resolvedServer = string.IsNullOrEmpty(regServer)
                    ? (_options.ServerName ?? string.Empty)
                    : regServer;
                _resolvedDatabase = string.IsNullOrEmpty(regDb)
                    ? (_options.DbName ?? string.Empty)
                    : regDb;

                SaveToRegistry(_resolvedServer, _resolvedDatabase);
            }

            _resolved = true;
        }

        private static void SaveToRegistry(string server, string db)
        {
            RegistryKey key = Registry.CurrentUser.CreateSubKey(@"DrurySoftware\BTRApp");
            key.SetValue("Server", server);
            key.SetValue("Database", db);
            key.Close();
        }

        public static (string, string) ReadFromRegistry()
        {
            RegistryKey key = Registry.CurrentUser.OpenSubKey(@"DrurySoftware\BTRApp");

            if (key != null)
            {
                string server = key.GetValue("Server") as string;
                string database = key.GetValue("Database") as string;
                key.Close();
                return (server ?? string.Empty, database ?? string.Empty);
            }

            return (string.Empty, string.Empty);
        }
    }
}
