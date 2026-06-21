using System;
using System.Collections.Generic;

namespace btr.application.SalesContext.FakturInfo
{
    public interface IBrgWarehouseConsumptionDal
    {
        IEnumerable<BrgWarehouseConsumptionDto> ListConsumptionByBrgWarehouse(
            DateTime windowStart,
            DateTime windowEnd);
    }

    public class BrgWarehouseConsumptionDto
    {
        public string BrgId { get; set; }

        public string WarehouseId { get; set; }

        public string WarehouseName { get; set; }

        public decimal SoldQty30 { get; set; }
    }
}
