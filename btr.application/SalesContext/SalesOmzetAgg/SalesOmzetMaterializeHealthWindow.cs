using System;
using btr.nuna.Domain;

namespace btr.application.SalesContext.SalesOmzetAgg
{
    public static class SalesOmzetMaterializeHealthWindow
    {
        public const int BucketDays = 60;

        public static Periode Resolve(DateTime windowEnd, int bucketDays = BucketDays)
        {
            var end = windowEnd.Date;
            return new Periode(end.AddDays(-bucketDays), end);
        }
    }
}
