using System;

namespace BtrReturGudang.Domain.Entities
{
    /// <summary>
    /// Local cache of customer master data.
    /// Represents BTRG_Customer table.
    /// Traceable to: LANDSCAPE Master Data Area, TD-003, BTR-RETUR-DESKTOP-001.
    /// </summary>
    public class Customer
    {
        public string CustomerId { get; set; }
        public string CustomerCode { get; set; }
        public string CustomerName { get; set; }
        public string Alamat { get; set; }
        public string Wilayah { get; set; }

        // Backwards-compatible aliases for legacy accessors
        public string CustId
        {
            get => CustomerId;
            set => CustomerId = value;
        }

        public string CustName
        {
            get => CustomerName;
            set => CustomerName = value;
        }

        public string CustCode
        {
            get => CustomerCode;
            set => CustomerCode = value;
        }

        public Customer()
        {
        }

        public Customer(
            string customerId,
            string customerName,
            string alamat = null,
            string customerCode = null,
            string wilayah = null)
        {
            CustomerId = customerId ?? string.Empty;
            CustomerName = customerName ?? string.Empty;
            Alamat = alamat;
            CustomerCode = customerCode;
            Wilayah = wilayah;
        }
    }
}
