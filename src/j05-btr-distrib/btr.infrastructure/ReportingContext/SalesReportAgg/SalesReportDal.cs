using System;
using System.Collections.Generic;
using System.Linq;
using btr.application.ReportingContext.SalesReportAgg.Contracts;
using btr.application.ReportingContext.SalesReportAgg.Queries;
using btr.application.SalesContext.FakturInfo;
using btr.application.SupportContext.TglJamAgg;
using btr.nuna.Domain;

namespace btr.infrastructure.ReportingContext.SalesReportAgg
{
    public class SalesReportDal : ISalesReportDal
    {
        private readonly IFakturViewDal _fakturViewDal;
        private readonly ITglJamDal _tglJamDal;

        public SalesReportDal(IFakturViewDal fakturViewDal, ITglJamDal tglJamDal)
        {
            _fakturViewDal = fakturViewDal;
            _tglJamDal = tglJamDal;
        }

        public SalesReportResponse GetReport(Periode periode)
        {
            var rows = _fakturViewDal.ListData(periode)?.ToList()
                       ?? new List<FakturView>();

            return new SalesReportResponse
            {
                PeriodFrom = periode.Tgl1,
                PeriodTo = periode.Tgl2,
                GeneratedAt = _tglJamDal.Now,
                Rows = rows
                    .OrderBy(r => r.Tgl)
                    .ThenBy(r => r.FakturCode)
                    .Select(MapRow)
                    .ToList()
            };
        }

        private static SalesReportRow MapRow(FakturView row)
        {
            return new SalesReportRow
            {
                FakturDate = row.Tgl.Date,
                FakturCode = row.FakturCode ?? string.Empty,
                CustomerName = row.Customer ?? string.Empty,
                SalesName = row.SalesPersonName ?? string.Empty,
                FakturTotal = row.GrandTotal,
                Status = ResolveStatus(row)
            };
        }

        private static string ResolveStatus(FakturView row)
        {
            if (row is null)
                return string.Empty;

            return row.StatusFaktur == 2 ? "Kembali" : string.Empty;
        }

    }
}
