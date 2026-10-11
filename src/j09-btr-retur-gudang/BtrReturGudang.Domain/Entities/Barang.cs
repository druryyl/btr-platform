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
        public string Satuan1 { get; private set; }
        public string Satuan2 { get; private set; }
        public string Satuan3 { get; private set; }
        public bool IsActive { get; private set; }

        public Barang()
        {
        }

        public Barang(string brgId, string brgName, string satuan1 = null, string satuan2 = null, string satuan3 = null, bool isActive = true)
        {
            BrgId = brgId;
            BrgName = brgName;
            Satuan1 = satuan1;
            Satuan2 = satuan2;
            Satuan3 = satuan3;
            IsActive = isActive;
        }

        /// <summary>
        /// Returns all non-empty packaging units configured for this item.
        /// </summary>
        public IEnumerable<string> GetAvailableUnits()
        {
            if (!string.IsNullOrWhiteSpace(Satuan1)) yield return Satuan1;
            if (!string.IsNullOrWhiteSpace(Satuan2)) yield return Satuan2;
            if (!string.IsNullOrWhiteSpace(Satuan3)) yield return Satuan3;
        }
    }
}
