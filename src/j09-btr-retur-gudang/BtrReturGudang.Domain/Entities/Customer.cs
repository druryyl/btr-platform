using System;

namespace BtrReturGudang.Domain.Entities
{
    /// <summary>
    /// Local cache of customer master data.
    /// Represents BTRG_Customer table.
    /// </summary>
    public class Customer
    {
        public string CustId { get; private set; }
        public string CustName { get; private set; }
        public string Alamat { get; private set; }

        public Customer()
        {
        }

        public Customer(string custId, string custName, string alamat = null)
        {
            CustId = custId;
            CustName = custName;
            Alamat = alamat;
        }
    }
}
