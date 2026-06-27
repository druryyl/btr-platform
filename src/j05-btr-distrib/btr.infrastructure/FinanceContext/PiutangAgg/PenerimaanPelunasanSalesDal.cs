using btr.application.FinanceContext.PiutangAgg.Contracts;
using btr.infrastructure.Helpers;
using btr.nuna.Domain;
using btr.nuna.Infrastructure;
using Dapper;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace btr.infrastructure.FinanceContext.PiutangAgg
{
    public class PenerimaanPelunasanSalesDal : IPenerimaanPelunasanSalesDal
    {
        private readonly DatabaseOptions _opt;

        public PenerimaanPelunasanSalesDal(IOptions<DatabaseOptions> opt)
        {
            _opt = opt.Value;
        }

        public IEnumerable<PenerimaanPelunasanSalesDto> ListData(Periode filter)
        {
            const string sql = @"
            SELECT
                daily.LunasDate,
                daily.SalesPersonId,
                daily.SalesName,
                SUM(daily.BayarTunai) AS BayarTunai,
                SUM(daily.BayarGiro) AS BayarGiro,
                SUM(ISNULL(hh.Retur, 0)) AS Retur,
                SUM(ISNULL(ii.Potongan, 0)) AS Potongan,
                SUM(ISNULL(jj.MateraiAdmin, 0)) AS MateraiAdmin,
                SUM(daily.BayarTunai + daily.BayarGiro) AS TotalBayar
            FROM (
                SELECT
                    CAST(aa.LunasDate AS DATE) AS LunasDate,
                    ISNULL(cc.SalesPersonId, '') AS SalesPersonId,
                    ISNULL(cc.SalesPersonName, '') AS SalesName,
                    aa.PiutangId,
                    SUM(CASE WHEN aa.JenisLunas = 0 THEN aa.Nilai ELSE 0 END) AS BayarTunai,
                    SUM(CASE WHEN aa.JenisLunas = 1 THEN aa.Nilai ELSE 0 END) AS BayarGiro
                FROM BTR_PiutangLunas aa
                LEFT JOIN BTR_Faktur bb ON aa.PiutangId = bb.FakturId
                LEFT JOIN BTR_SalesPerson cc ON bb.SalesPersonId = cc.SalesPersonId
                WHERE aa.LunasDate BETWEEN @Tgl1 AND @Tgl2
                GROUP BY
                    CAST(aa.LunasDate AS DATE),
                    cc.SalesPersonId,
                    cc.SalesPersonName,
                    aa.PiutangId
            ) daily
            LEFT JOIN (
                SELECT PiutangId, SUM(aa1.NilaiPlus - aa1.NilaiMinus) Retur
                FROM BTR_PiutangElement aa1
                WHERE aa1.ElementName = 'Retur'
                GROUP BY PiutangId) hh ON daily.PiutangId = hh.PiutangId
            LEFT JOIN (
                SELECT PiutangId, SUM(aa1.NilaiPlus - aa1.NilaiMinus) Potongan
                FROM BTR_PiutangElement aa1
                WHERE aa1.ElementName = 'Potongan'
                GROUP BY PiutangId) ii ON daily.PiutangId = ii.PiutangId
            LEFT JOIN (
                SELECT PiutangId, SUM(aa1.NilaiPlus - aa1.NilaiMinus) MateraiAdmin
                FROM BTR_PiutangElement aa1
                WHERE aa1.ElementName = 'Materai' OR aa1.ElementName = 'Admin'
                GROUP BY PiutangId) jj ON daily.PiutangId = jj.PiutangId
            GROUP BY
                daily.LunasDate,
                daily.SalesPersonId,
                daily.SalesName ";

            //  parameter
            var dp = new DynamicParameters();
            dp.AddParam("@Tgl1", filter.Tgl1, SqlDbType.DateTime);
            dp.AddParam("@Tgl2", filter.Tgl2, SqlDbType.DateTime);

            using(var conn = new SqlConnection(ConnStringHelper.Get(_opt)))
            {
                return conn.Read<PenerimaanPelunasanSalesDto>(sql, dp);
            }
        }
    }
}
