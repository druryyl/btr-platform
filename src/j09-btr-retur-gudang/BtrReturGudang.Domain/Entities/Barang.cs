using System;
using System.Collections.Generic;

namespace BtrReturGudang.Domain.Entities
{
    /// <summary>
    /// Local cache of product master data.
    /// Represents BTRG_Barang table.
    /// </summary>
    public class Barang
    {
        public string BrgId { get; private set; }
        public string BrgName { get; private set; }
        public string SatBesar { get; private set; }
        public string SatKecil { get; private set; }
        public int Konversi { get; private set; }
        public bool IsActive { get; private set; }

        public Barang()
        {
            Konversi = 1;
        }

        public Barang(string brgId, string brgName, string satBesar = null, string satKecil = null, int konversi = 1, bool isActive = true)
        {
            BrgId = brgId;
            BrgName = brgName;
            SatBesar = satBesar;
            SatKecil = satKecil;
            Konversi = konversi <= 0 ? 1 : konversi;
            IsActive = isActive;
        }

        /// <summary>
        /// Returns all non-empty, distinct packaging units configured for this item.
        /// </summary>
        public IEnumerable<string> GetAvailableUnits()
        {
            if (!string.IsNullOrWhiteSpace(SatBesar))
                yield return SatBesar;
            if (!string.IsNullOrWhiteSpace(SatKecil) && !string.Equals(SatBesar, SatKecil, StringComparison.OrdinalIgnoreCase))
                yield return SatKecil;
        }

        /// <summary>
        /// Resolves the conversion multiplier to base unit (Pcs) for the given packaging unit.
        /// Returns Konversi for SatBesar, 1 for SatKecil, and 1 as fallback.
        /// </summary>
        public int GetConversionForUnit(string satuan)
        {
            if (string.IsNullOrWhiteSpace(satuan)) return 1;

            if (!string.IsNullOrWhiteSpace(SatBesar) && string.Equals(satuan, SatBesar, StringComparison.OrdinalIgnoreCase))
            {
                return Konversi > 0 ? Konversi : 1;
            }

            if (!string.IsNullOrWhiteSpace(SatKecil) && string.Equals(satuan, SatKecil, StringComparison.OrdinalIgnoreCase))
            {
                return 1;
            }

            return 1;
        }
    }
}
