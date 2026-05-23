using System;
using btr.domain.SalesContext.SalesOmzetAgg;
using btr.nuna.Domain;

namespace btr.application.SalesContext.SalesOmzetAgg.Policies
{
    public class SalesOmzetPeriodPolicy : ISalesOmzetPeriodPolicy
    {
        public bool IsInPeriod(SalesOmzetModel row, Periode periode, SalesOmzetPeriodFilterMode mode)
        {
            if (row is null) return false;

            switch (mode)
            {
                case SalesOmzetPeriodFilterMode.OmzetPeriod:
                    return !SalesOmzetDates.IsSentinel(row.OmzetDate)
                           && row.OmzetDate.Date >= periode.Tgl1.Date
                           && row.OmzetDate.Date <= periode.Tgl2.Date;

                case SalesOmzetPeriodFilterMode.SalesPeriod:
                    return row.SalesDate.Date >= periode.Tgl1.Date
                           && row.SalesDate.Date <= periode.Tgl2.Date;

                default:
                    throw new ArgumentOutOfRangeException(nameof(mode), mode, null);
            }
        }

        public string ToSqlWhere(SalesOmzetPeriodFilterMode mode)
        {
            switch (mode)
            {
                case SalesOmzetPeriodFilterMode.OmzetPeriod:
                    return @"OmzetDate BETWEEN @Tgl1 AND @Tgl2 AND OmzetDate <> '3000-01-01'";

                case SalesOmzetPeriodFilterMode.SalesPeriod:
                    return @"SalesDate BETWEEN @Tgl1 AND @Tgl2";

                default:
                    throw new ArgumentOutOfRangeException(nameof(mode), mode, null);
            }
        }
    }
}
