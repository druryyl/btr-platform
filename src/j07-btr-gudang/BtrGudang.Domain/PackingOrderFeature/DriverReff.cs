using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BtrGudang.Domain.PackingOrderFeature
{
    public class DriverReff
    {
        public DriverReff(string driverId, string driverName)
        {
            DriverId = driverId;
            DriverName = driverName;
        }
        public static DriverReff Default => new DriverReff("", "");
        public string DriverId { get; private set; }
        public string DriverName { get; private set; }
    }
}
