using System;

namespace btr.application.SalesContext.FakturInfo
{
    public interface IBrgLastFakturDal
    {
        System.Collections.Generic.IEnumerable<BrgLastFakturDto> ListLastFakturByBrg();
    }

    public class BrgLastFakturDto
    {
        public string BrgId { get; set; }

        public string BrgCode { get; set; }

        public string BrgName { get; set; }

        public DateTime LastFakturDate { get; set; }
    }
}
