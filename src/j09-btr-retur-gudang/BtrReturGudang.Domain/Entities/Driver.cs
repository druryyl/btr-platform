using System;

namespace BtrReturGudang.Domain.Entities
{
    /// <summary>
    /// Local cache of delivery drivers.
    /// Represents BTRG_Driver table.
    /// </summary>
    public class Driver
    {
        public string DriverId { get; private set; }
        public string DriverName { get; private set; }

        public Driver()
        {
        }

        public Driver(string driverId, string driverName)
        {
            DriverId = driverId;
            DriverName = driverName;
        }
    }
}
