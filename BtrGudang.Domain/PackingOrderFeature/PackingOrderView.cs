using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BtrGudang.Domain.PackingOrderFeature
{
    public class PackingOrderView : IPackingOrderKey
    {
        public string PackingOrderId { get; set; }
        public string FakturCode { get; set;  }
        public DateTime FakturDate { get; set; }
        public decimal GrandTotal { get; set; }
        public string DriverId { get; set; }
        public string DriverName { get; set; }
        public string CustomerCode { get; set;  }
        public string CustomerName { get; set; }    
        public string Alamat { get; set; }
        public DateTime DownloadTimestamp { get; set; }
        public string PrintLogId { get; set; }
        public DateTime JamCetak { get; set;  }
        public string JenisCetak { get; set; }
        public string Note { get; set;  }
    }
}
