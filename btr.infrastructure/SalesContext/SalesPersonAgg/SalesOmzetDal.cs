using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using btr.application.SalesContext.OrderFeature;
using btr.application.SalesContext.SalesOmzetAgg;
using btr.application.SalesContext.SalesOmzetAgg.Policies;
using btr.domain.SalesContext.SalesOmzetAgg;
using btr.infrastructure.Helpers;
using btr.nuna.Domain;
using btr.nuna.Infrastructure;
using Dapper;
using Microsoft.Extensions.Options;

namespace btr.infrastructure.SalesContext.SalesPersonAgg
{
    public class SalesOmzetDal : ISalesOmzetDal
    {
        private readonly DatabaseOptions _opt;
        private readonly ISalesOmzetPeriodPolicy _periodPolicy;

        public SalesOmzetDal(IOptions<DatabaseOptions> opt, ISalesOmzetPeriodPolicy periodPolicy)
        {
            _opt = opt.Value;
            _periodPolicy = periodPolicy;
        }

        public IEnumerable<SalesOmzetView> ListData(Periode filter)
        {
            return ListData(filter, SalesOmzetPeriodFilterMode.OmzetPeriod);
        }

        public IEnumerable<SalesOmzetView> ListData(Periode periode, SalesOmzetPeriodFilterMode mode)
        {
            var periodWhere = _periodPolicy.ToSqlWhere(mode);
            var sql = $@"
                SELECT
                    SalesPersonName,
                    OrderId,
                    OrderDate,
                    OrderTotal,
                    FakturCode,
                    FakturDate,
                    FakturTotal,
                    CustomerName,
                    Code,
                    Alamat,
                    OmzetDate,
                    OmzetStatus,
                    SaleKind
                FROM BTR_SalesOmzet
                WHERE {periodWhere}
                  AND OmzetStatus <> 'Void'
                ORDER BY SalesPersonName, OrderDate, FakturDate";

            var dp = new DynamicParameters();
            dp.AddParam("@Tgl1", periode.Tgl1, SqlDbType.DateTime);
            dp.AddParam("@Tgl2", periode.Tgl2, SqlDbType.DateTime);

            using (var conn = new SqlConnection(ConnStringHelper.Get(_opt)))
            {
                var rows = conn.Read<SalesOmzetListRow>(sql, dp);
                return MapToView(rows);
            }
        }

        private static IEnumerable<SalesOmzetView> MapToView(IEnumerable<SalesOmzetListRow> rows)
        {
            foreach (var row in rows)
            {
                yield return new SalesOmzetView
                {
                    SalesPersonName = row.SalesPersonName,
                    OrderId = row.OrderId,
                    OrderDate = FormatDisplayDate(row.OrderDate),
                    OrderTotal = row.OrderTotal,
                    FakturCode = row.FakturCode,
                    FakturDate = FormatDisplayDate(row.FakturDate),
                    FakturTotal = row.FakturTotal,
                    CustomerName = row.CustomerName,
                    Code = row.Code,
                    Alamat = row.Alamat,
                    OmzetDate = FormatDisplayDate(row.OmzetDate),
                    OmzetStatus = ParseEnum(row.OmzetStatus, SalesOmzetStatusEnum.Outstanding),
                    SaleKind = ParseEnum(row.SaleKind, SaleKindEnum.OrderedSale)
                };
            }
        }

        private static DateTime FormatDisplayDate(DateTime value) =>
            SalesOmzetDates.IsSentinel(value) ? DateTime.MinValue : value;

        private static TEnum ParseEnum<TEnum>(string value, TEnum fallback)
            where TEnum : struct
        {
            if (string.IsNullOrEmpty(value))
                return fallback;

            return Enum.TryParse(value, out TEnum parsed) ? parsed : fallback;
        }

        private sealed class SalesOmzetListRow
        {
            public string SalesPersonName { get; set; }
            public string OrderId { get; set; }
            public DateTime OrderDate { get; set; }
            public decimal OrderTotal { get; set; }
            public string FakturCode { get; set; }
            public DateTime FakturDate { get; set; }
            public decimal FakturTotal { get; set; }
            public string CustomerName { get; set; }
            public string Code { get; set; }
            public string Alamat { get; set; }
            public DateTime OmzetDate { get; set; }
            public string OmzetStatus { get; set; }
            public string SaleKind { get; set; }
        }
    }
}
