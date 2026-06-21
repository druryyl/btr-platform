using System;
using System.Collections.Generic;

namespace btr.application.SalesContext.FakturInfo
{
    public interface IBrgConsumptionDal
    {
        IEnumerable<BrgConsumptionDto> ListConsumptionByBrg(
            DateTime window30Start,
            DateTime window90Start,
            DateTime windowEnd);

        IEnumerable<DailyCompanyConsumptionDto> ListDailyCompanyConsumption(
            DateTime windowStart,
            DateTime windowEnd);
    }

    public class BrgConsumptionDto
    {
        public string BrgId { get; set; }

        public decimal SoldQty30 { get; set; }

        public decimal SoldQty90 { get; set; }

        public DateTime? FirstFakturDate { get; set; }

        public bool IsAktif { get; set; }
    }

    public class DailyCompanyConsumptionDto
    {
        public DateTime FakturDate { get; set; }

        public decimal UnitsSold { get; set; }
    }
}
