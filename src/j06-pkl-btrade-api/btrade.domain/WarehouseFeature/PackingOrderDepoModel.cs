using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace btrade.domain.WarehouseFeature
{
    public class PackingOrderDepoModel : IPackingOrderKey
    {
        public PackingOrderDepoModel(string packingOrderId, string depoId,
            DateTime updateTimestamp, DateTime downloadTimestamp)
        {
            PackingOrderId = packingOrderId;
            DepoId = depoId;
            UpdateTimestamp = updateTimestamp;
            DownloadTimestamp = downloadTimestamp;
        }

        public void DownloadedAt(DateTime timestamp)
        {
            DownloadTimestamp = timestamp;
        }

        public string PackingOrderId { get; }
        public string DepoId { get; }
        public DateTime UpdateTimestamp { get; }
        public DateTime DownloadTimestamp { get; private set; }
    }
}
