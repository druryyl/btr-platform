using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace j07_btrade_sync.Model
{
    public class ReturnOrderItemType : IReturnOrderKey
    {
        public string ReturnOrderId { get; set; }
        public int NoUrut { get; set; }
        public string BrgId { get; set; }
        public string BrgCode { get; set; }
        public string BrgName { get; set; }
        public decimal Qty { get; set; }
        public string SatId { get; set; }
        public string JenisRetur { get; set; }
    }
}
