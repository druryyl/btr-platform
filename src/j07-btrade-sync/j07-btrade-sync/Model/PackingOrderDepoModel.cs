using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace j07_btrade_sync.Model
{
    public class PackingOrderDepoModel : IPackingOrderKey
    {
        public PackingOrderDepoModel(string packingOrderId, string depoId)
        {
            PackingOrderId = packingOrderId;
            DepoId = depoId;
        }
        public string PackingOrderId { get; }
        public string DepoId { get; }
    }
}
