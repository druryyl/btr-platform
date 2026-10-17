using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Microsoft.Win32;

namespace BtrReturGudang.Infrastructure.Helpers
{
    /// <summary>
    /// Helper for resolving and creating SQL Server database connections targeting the local warehouse database (btrgd).
    /// Resolution order:
    /// 1. In-memory override (via SetConnectionString)
    /// 2. Windows Registry (HKCU\Software\BTR\Warehouse -> DbConnectionString)
    /// 3. App.config ConnectionStrings ("btrgd")
    /// 4. App.config AppSettings ("BtrgdConnString" or "btrgd")
    /// 5. Standard default fallback: Server=(local);Database=btrgd;User Id=btrGudangLogin;Password=btrGudang123!;Max Pool Size=20;
    /// Traceable to: ARCHITECTURE §5.1, P2-S04.
    /// </summary>
    public static class ConnStringHelper
    {
        public const string DefaultConnectionString =
            "Server=(local);Database=btrgd;User Id=btrGudangLogin;Password=btrGudang123!;Max Pool Size=20;";

        private const string RegistryKeyPath = @"Software\BTR\Warehouse";
        private const string RegistryValueName = "DbConnectionString";

        private static string _overrideConnectionString;

        /// <summary>
        /// Explicitly overrides the connection string for the current application session or test execution.
        /// </summary>
        public static void SetConnectionString(string connectionString)
        {
            _overrideConnectionString = connectionString;
        }

        /// <summary>
        /// Resets any in-memory override, restoring the default resolution chain.
        /// </summary>
        public static void ResetConnectionString()
        {
            _overrideConnectionString = null;
        }

        /// <summary>
        /// Resolves the active connection string.
        /// </summary>
        public static string GetConnectionString()
        {
            if (!string.IsNullOrWhiteSpace(_overrideConnectionString))
            {
                return _overrideConnectionString;
            }

            // Check Registry (HKCU first, then HKLM)
            try
            {
                using (var key = Registry.CurrentUser.OpenSubKey(RegistryKeyPath))
                {
                    var val = key?.GetValue(RegistryValueName) as string;
                    if (!string.IsNullOrWhiteSpace(val))
                        return val;
                }

                using (var key = Registry.LocalMachine.OpenSubKey(RegistryKeyPath))
                {
                    var val = key?.GetValue(RegistryValueName) as string;
                    if (!string.IsNullOrWhiteSpace(val))
                        return val;
                }
            }
            catch
            {
                // Registry access denied or unavailable; proceed to next fallback
            }

            // Check App.config ConnectionStrings
            try
            {
                var connSetting = ConfigurationManager.ConnectionStrings["btrgd"];
                if (connSetting != null && !string.IsNullOrWhiteSpace(connSetting.ConnectionString))
                {
                    return connSetting.ConnectionString;
                }
            }
            catch
            {
                // ConfigurationManager unavailable or error; proceed
            }

            // Check App.config AppSettings
            try
            {
                var appSetting = ConfigurationManager.AppSettings["BtrgdConnString"]
                              ?? ConfigurationManager.AppSettings["btrgd"];
                if (!string.IsNullOrWhiteSpace(appSetting))
                {
                    return appSetting;
                }
            }
            catch
            {
                // ConfigurationManager unavailable; proceed
            }

            return DefaultConnectionString;
        }

        /// <summary>
        /// Creates a new short-lived, unopened SqlConnection.
        /// </summary>
        public static SqlConnection CreateConnection(string connectionString = null)
        {
            var connStr = !string.IsNullOrWhiteSpace(connectionString) ? connectionString : GetConnectionString();
            return new SqlConnection(connStr);
        }

        /// <summary>
        /// Creates and opens a new short-lived SqlConnection synchronously.
        /// </summary>
        public static SqlConnection OpenConnection(string connectionString = null)
        {
            var conn = CreateConnection(connectionString);
            conn.Open();
            return conn;
        }

        /// <summary>
        /// Creates and opens a new short-lived SqlConnection asynchronously.
        /// </summary>
        public static async Task<SqlConnection> OpenConnectionAsync(string connectionString = null)
        {
            var conn = CreateConnection(connectionString);
            await conn.OpenAsync().ConfigureAwait(false);
            return conn;
        }
    }
}
