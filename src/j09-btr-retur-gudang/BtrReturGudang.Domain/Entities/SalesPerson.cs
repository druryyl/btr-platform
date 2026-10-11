using System;

namespace BtrReturGudang.Domain.Entities
{
    /// <summary>
    /// Local cache of sales personnel.
    /// Represents BTRG_SalesPerson table.
    /// </summary>
    public class SalesPerson
    {
        public string SalesPersonId { get; private set; }
        public string SalesPersonName { get; private set; }

        public SalesPerson()
        {
        }

        public SalesPerson(string salesPersonId, string salesPersonName)
        {
            SalesPersonId = salesPersonId;
            SalesPersonName = salesPersonName;
        }
    }
}
