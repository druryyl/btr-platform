using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace j07_btrade_sync.Model
{
    public class DriverType
    {
        public DriverType(string driverId, string driverName, bool isAktif, string serverId)
        {
            DriverId = driverId;
            DriverName = driverName;
            IsAktif = isAktif;
            ServerId = serverId;
        }
        public string DriverId { get; set; }
        public string DriverName { get; set; }
        public bool IsAktif { get; set; }
        public string ServerId { get; set; }
    }
}
