using System;
using System.Collections.Generic;
using System.Linq;
using btr.application.FinanceContext.PiutangAgg.Contracts;
using btr.application.ReportingContext.PiutangReportAgg;
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

        public PiutangReportResponse GetReport(Periode periode, PiutangReportDateField dateField)
        {
            var rows = _piutangSalesWilayahDal.ListData(periode, dateField)?.ToList()
                       ?? new List<PiutangSalesWilayahDto>();

            return BuildResponse(rows, periode.Tgl1, periode.Tgl2, dateField, allOpenBalances: false);
        }

        public PiutangReportResponse GetAllOpenBalancesReport(PiutangReportDateField dateField)
        {
            var rows = _piutangSalesWilayahDal.ListAllOpenBalances()?.ToList()
                       ?? new List<PiutangSalesWilayahDto>();

            return BuildResponse(
                rows,
                new DateTime(1900, 1, 1),
                new DateTime(1900, 1, 1),
                dateField,
                allOpenBalances: true);
        }

        private PiutangReportResponse BuildResponse(
            List<PiutangSalesWilayahDto> rows,
            DateTime periodFrom,
            DateTime periodTo,
            PiutangReportDateField dateField,
            bool allOpenBalances)
        {
            var outstanding = rows.Where(r => r.KurangBayar > 1).ToList();

            return new PiutangReportResponse
            {
                PeriodFrom = periodFrom,
                PeriodTo = periodTo,
                DateField = dateField.ToString(),
                AllOpenBalances = allOpenBalances,
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
            CustomerCode = row.CustomerCode ?? string.Empty,
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
