using System;
using Microsoft.Win32;
using BtrReturGudang.AppTier.Contracts;

namespace BtrReturGudang.Infrastructure.Helpers
{
    /// <summary>
    /// Reads and persists workstation configuration under HKCU\Software\BTRApp\ReturGudang.
    /// Manages Depo/Warehouse code, ServerTargetId, active Operator name and Google email,
    /// and sound feedback toggle.
    /// Traceable to: ARCHITECTURE §5.1, §7, TD-007, TD-009, TD-012, P2-S06.
    /// </summary>
    public class RegistryHelper : IRegistryHelper
    {
        public const string SubKeyPath = @"Software\BTRApp\ReturGudang";

        public const string KeyWarehouseCode = "WarehouseCode";
        public const string KeyServerTargetId = "ServerTargetId";
        public const string KeyApiBaseUrl = "ApiBaseUrl";
        public const string KeyLastOperatorName = "LastOperatorName";
        public const string KeyLastOperatorEmail = "LastOperatorEmail";
        public const string KeyEnableSoundFeedback = "EnableSoundFeedback";

        public const string DefaultApiBaseUrl = "http://dev.smart-ics.com:8089/belajar-api/";
        public const string DefaultServerTargetId = "JOG";

        /// <summary>
        /// Normalizes an API base URL by trimming surrounding whitespace and guaranteeing
        /// a single trailing slash. RestSharp strips the last path segment (e.g. '/belajar-api')
        /// when resolving relative resources against a base URL that lacks a trailing slash.
        /// Returns an empty string for null/whitespace input.
        /// Traceable to: ARCHITECTURE-NOT-REQUIRED, P2-S02.
        /// </summary>
        public static string NormalizeApiUrl(string url)
        {
            if (string.IsNullOrWhiteSpace(url)) return string.Empty;
            var trimmed = url.Trim();
            return trimmed.EndsWith("/", StringComparison.Ordinal) ? trimmed : trimmed + "/";
        }

        /// <summary>
        /// Workstation warehouse / depo code (e.g. 'GDG-PST', 'GDG-SBY').
        /// </summary>
        public string WarehouseCode
        {
            get => ReadString(KeyWarehouseCode, string.Empty);
            set => WriteString(KeyWarehouseCode, value);
        }

        /// <summary>
        /// Target server ID for multi-tenant / multi-depo routing.
        /// Defaults to 'JOG'.
        /// </summary>
        public string ServerTargetId
        {
            get
            {
                var val = ReadString(KeyServerTargetId, DefaultServerTargetId);
                return string.IsNullOrWhiteSpace(val) ? DefaultServerTargetId : val;
            }
            set => WriteString(KeyServerTargetId, string.IsNullOrWhiteSpace(value) ? DefaultServerTargetId : value);
        }

        /// <summary>
        /// Cloud API relay base URL (e.g. 'http://dev.smart-ics.com:8089/belajar-api').
        /// </summary>
        public string ApiBaseUrl
        {
            get
            {
                var val = ReadString(KeyApiBaseUrl, DefaultApiBaseUrl);
                return NormalizeApiUrl(string.IsNullOrWhiteSpace(val) ? DefaultApiBaseUrl : val);
            }
            set => WriteString(KeyApiBaseUrl, NormalizeApiUrl(string.IsNullOrWhiteSpace(value) ? DefaultApiBaseUrl : value));
        }

        /// <summary>
        /// Name of last active shift operator.
        /// Retained for on-screen display and local record attribution only.
        /// </summary>
        public string LastOperatorName
        {
            get => ReadString(KeyLastOperatorName, string.Empty);
            set => WriteString(KeyLastOperatorName, value);
        }

        /// <summary>
        /// Registered Google email of the last active shift operator.
        /// Carried as X-Session-Actor and resolved by the Cloud to the authoritative BTR user.
        /// Returns an empty string when unset.
        /// Traceable to: ARCHITECTURE TD-007, TD-012, P1-S02.
        /// </summary>
        public string LastOperatorEmail
        {
            get
            {
                var val = ReadString(KeyLastOperatorEmail, string.Empty);
                return string.IsNullOrWhiteSpace(val) ? string.Empty : val.Trim();
            }
            set => WriteString(KeyLastOperatorEmail, value == null ? string.Empty : value.Trim());
        }

        /// <summary>
        /// Audio feedback toggle persisted in workstation registry.
        /// Synchronizes with AudioHelper.EnableSoundFeedback.
        /// </summary>
        public bool EnableSoundFeedback
        {
            get => ReadBool(KeyEnableSoundFeedback, true);
            set
            {
                WriteBool(KeyEnableSoundFeedback, value);
                AudioHelper.EnableSoundFeedback = value;
            }
        }

        /// <summary>
        /// Reads a string value from HKCU\Software\BTRApp\ReturGudang.
        /// </summary>
        public static string ReadString(string keyName, string defaultValue = "")
        {
            try
            {
                using (var key = Registry.CurrentUser.OpenSubKey(SubKeyPath, writable: false))
                {
                    if (key == null) return defaultValue;
                    var val = key.GetValue(keyName);
                    if (val == null) return defaultValue;
                    var str = val.ToString();
                    return string.IsNullOrWhiteSpace(str) && !string.IsNullOrWhiteSpace(defaultValue)
                        ? defaultValue
                        : str;
                }
            }
            catch
            {
                return defaultValue;
            }
        }

        /// <summary>
        /// Writes a string value to HKCU\Software\BTRApp\ReturGudang.
        /// </summary>
        public static void WriteString(string keyName, string value)
        {
            try
            {
                using (var key = Registry.CurrentUser.CreateSubKey(SubKeyPath))
                {
                    if (key != null)
                    {
                        key.SetValue(keyName, value ?? string.Empty, RegistryValueKind.String);
                    }
                }
            }
            catch
            {
                // Graceful fallback if registry permissions are restricted
            }
        }

        /// <summary>
        /// Reads a boolean value from HKCU\Software\BTRApp\ReturGudang.
        /// </summary>
        public static bool ReadBool(string keyName, bool defaultValue = true)
        {
            try
            {
                using (var key = Registry.CurrentUser.OpenSubKey(SubKeyPath, writable: false))
                {
                    if (key == null) return defaultValue;
                    var val = key.GetValue(keyName);
                    if (val == null) return defaultValue;

                    if (val is int intVal) return intVal != 0;
                    if (bool.TryParse(val.ToString(), out bool boolVal)) return boolVal;
                    if (int.TryParse(val.ToString(), out int parsedInt)) return parsedInt != 0;

                    return defaultValue;
                }
            }
            catch
            {
                return defaultValue;
            }
        }

        /// <summary>
        /// Writes a boolean value as DWORD to HKCU\Software\BTRApp\ReturGudang.
        /// </summary>
        public static void WriteBool(string keyName, bool value)
        {
            try
            {
                using (var key = Registry.CurrentUser.CreateSubKey(SubKeyPath))
                {
                    if (key != null)
                    {
                        key.SetValue(keyName, value ? 1 : 0, RegistryValueKind.DWord);
                    }
                }
            }
            catch
            {
                // Graceful fallback
            }
        }

        /// <summary>
        /// Deletes the workstation registry configuration (useful for test resets).
        /// </summary>
        public static void DeleteConfiguration()
        {
            try
            {
                Registry.CurrentUser.DeleteSubKeyTree(SubKeyPath, false);
            }
            catch
            {
            }
        }
    }
}
