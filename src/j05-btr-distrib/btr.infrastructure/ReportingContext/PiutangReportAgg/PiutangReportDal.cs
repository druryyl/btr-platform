using System;
using System.Collections.Generic;
using System.Linq;
using btr.application.FinanceContext.PiutangAgg.Contracts;
using btr.application.ReportingContext.PiutangReportAgg.Contracts;
using btr.application.ReportingContext.PiutangReportAgg.Queries;
using btr.application.SupportContext.TglJamAgg;
using btr.nuna.Domain;

namespace btr.infrastructure.ReportingContext.PiutangReportAgg
{
    public class PiutangReportDal : IPiutangReportDal
    {
        private readonly IPiutangSalesWilayahDal _piutangSalesWilayahDal;
        private readonly ITglJamDal _tglJamDal;

        public PiutangReportDal(
            IPiutangSalesWilayahDal piutangSalesWilayahDal,
            ITglJamDal tglJamDal)
        {
            _piutangSalesWilayahDal = piutangSalesWilayahDal;
            _tglJamDal = tglJamDal;
        }

        public PiutangReportResponse GetReport()
        {
            var periode = OpenReceivablesPeriode();
            var rows = _piutangSalesWilayahDal.ListData(periode)?.ToList()
                       ?? new List<PiutangSalesWilayahDto>();

            var outstanding = rows.Where(r => r.KurangBayar > 1).ToList();

            return new PiutangReportResponse
            {
                PeriodFrom = periode.Tgl1,
                PeriodTo = periode.Tgl2,
                GeneratedAt = _tglJamDal.Now,
                Summary = new PiutangReportSummary
                {
                    TotalPiutang = outstanding.Sum(r => r.KurangBayar),
                    TotalCustomer = outstanding
                        .Select(ResolveCustomerKey)
                        .Where(key => key.Length > 0)
                        .Distinct(StringComparer.OrdinalIgnoreCase)
                        .Count()
                },
                Rows = outstanding
                    .OrderBy(r => r.CustomerName)
                    .ThenBy(r => r.FakturDate)
                    .ThenBy(r => r.FakturCode)
                    .Select(MapRow)
                    .ToList()
            };
        }

        private Periode OpenReceivablesPeriode()
        {
            var today = _tglJamDal.Now.Date;
            return new Periode(new DateTime(2000, 1, 1), today);
        }

        private static string ResolveCustomerKey(PiutangSalesWilayahDto row)
        {
            if (row is null)
                return string.Empty;

            if (!string.IsNullOrWhiteSpace(row.CustomerCode))
                return row.CustomerCode.Trim();

            return row.CustomerName?.Trim() ?? string.Empty;
        }

        private static PiutangReportRow MapRow(PiutangSalesWilayahDto row) => new PiutangReportRow
        {
            CustomerName = row.CustomerName ?? string.Empty,
            SalesName = row.SalesName ?? string.Empty,
            FakturCode = row.FakturCode ?? string.Empty,
            FakturDate = row.FakturDate.Date,
            JatuhTempo = row.JatuhTempo.Date,
            TotalJual = row.TotalJual,
            KurangBayar = row.KurangBayar,
        };
    }
}
