using System;
using Microsoft.Win32;
using BtrReturGudang.AppTier.Contracts;

namespace BtrReturGudang.Infrastructure.Helpers
{
    /// <summary>
    /// Reads and persists workstation configuration under HKCU\Software\BTRApp\ReturGudang.
    /// Manages Depo/Warehouse code, ServerTargetId, active Operator name, and sound feedback toggle.
    /// Traceable to: ARCHITECTURE §5.1, §7, TD-007, TD-009, P2-S06.
    /// </summary>
    public class RegistryHelper : IRegistryHelper
    {
        public const string SubKeyPath = @"Software\BTRApp\ReturGudang";

        public const string KeyWarehouseCode = "WarehouseCode";
        public const string KeyServerTargetId = "ServerTargetId";
        public const string KeyLastOperatorName = "LastOperatorName";
        public const string KeyEnableSoundFeedback = "EnableSoundFeedback";

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
        /// </summary>
        public string ServerTargetId
        {
            get => ReadString(KeyServerTargetId, string.Empty);
            set => WriteString(KeyServerTargetId, value);
        }

        /// <summary>
        /// Name of last active shift operator.
        /// </summary>
        public string LastOperatorName
        {
            get => ReadString(KeyLastOperatorName, string.Empty);
            set => WriteString(KeyLastOperatorName, value);
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
                    return val != null ? val.ToString() : defaultValue;
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
