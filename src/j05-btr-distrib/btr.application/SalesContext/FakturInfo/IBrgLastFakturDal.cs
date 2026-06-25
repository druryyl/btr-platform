using System;
using System.Collections.Generic;

namespace btr.application.SalesContext.FakturInfo
{
    public interface IBrgLastFakturDal
    {
        IEnumerable<BrgLastFakturDto> ListLastFakturByBrg();

        IEnumerable<BrgLastFakturDto> ListLastFakturByBrgAsOf(DateTime asOfDate);
    }

    public class BrgLastFakturDto
    {
        public string BrgId { get; set; }

        public string BrgCode { get; set; }

        public string BrgName { get; set; }

        public DateTime LastFakturDate { get; set; }
    }
}
