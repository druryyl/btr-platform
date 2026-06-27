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
                CAST(aa.LunasDate AS DATE) AS LunasDate,
                ISNULL(cc.SalesPersonId, '') AS SalesPersonId,
                ISNULL(cc.SalesPersonName, '') AS SalesName,
                SUM(ISNULL(ff.BayarTunai, 0)) AS BayarTunai,
                SUM(ISNULL(gg.BayarGiro, 0)) AS BayarGiro,
                SUM(ISNULL(hh.Retur, 0)) AS Retur,
                SUM(ISNULL(ii.Potongan, 0)) AS Potongan,
                SUM(ISNULL(jj.MateraiAdmin, 0)) AS MateraiAdmin,
                SUM(ISNULL(ff.BayarTunai, 0) + ISNULL(gg.BayarGiro,0)) AS TotalBayar

            FROM 
                BTR_PiutangLunas aa
                LEFT JOIN BTR_Faktur bb ON aa.PiutangId = bb.FakturId
                LEFT JOIN BTR_SalesPerson cc ON bb.SalesPersonId = cc.SalesPersonId
                LEFT JOIN (
                    SELECT  PiutangId, SUM(aa1.Nilai) BayarTunai
                    FROM BTR_PiutangLunas aa1
                    WHERE aa1.JenisLunas = 0 
                        AND aa1.LunasDate BETWEEN @Tgl1 AND @Tgl2
                    GROUP BY PiutangId) ff ON aa.PiutangId = ff.PiutangId
                LEFT JOIN (
                    SELECT  PiutangId, SUM(aa1.Nilai) BayarGiro
                    FROM BTR_PiutangLunas aa1
                    WHERE aa1.JenisLunas = 1 
                        AND aa1.LunasDate BETWEEN @Tgl1 AND @Tgl2
                     GROUP BY PiutangId) gg ON aa.PiutangId = gg.PiutangId
                LEFT JOIN (
                    SELECT  PiutangId, SUM(aa1.NilaiPlus - aa1.NilaiMinus) Retur
                    FROM BTR_PiutangElement aa1
                    WHERE aa1.ElementName = 'Retur' 
                    GROUP BY PiutangId) hh ON aa.PiutangId = hh.PiutangId
                LEFT JOIN (
                    SELECT  PiutangId, SUM(aa1.NilaiPlus - aa1.NilaiMinus) Potongan
                    FROM BTR_PiutangElement aa1
                    WHERE aa1.ElementName = 'Potongan' 
                    GROUP BY PiutangId) ii ON aa.PiutangId = ii.PiutangId
                LEFT JOIN (
                    SELECT  PiutangId, SUM(aa1.NilaiPlus - aa1.NilaiMinus) MateraiAdmin
                    FROM BTR_PiutangElement aa1
                    WHERE aa1.ElementName = 'Materai' OR aa1.ElementName = 'Admin' 
                    GROUP BY PiutangId) jj ON aa.PiutangId = jj.PiutangId
             WHERE
                 aa.LunasDate BETWEEN @Tgl1 AND @Tgl2
            GROUP BY
                CAST(aa.LunasDate AS DATE), cc.SalesPersonId, cc.SalesPersonName ";

            //  parameter
            var dp = new DynamicParameters();
            dp.AddParam("@Tgl1", filter.Tgl1, SqlDbType.DateTime);
            dp.AddParam("@Tgl2", filter.Tgl2, SqlDbType.DateTime);

            using(var conn = new SqlConnection(ConnStringHelper.Get(_opt)))
            {
                return conn.Read<PenerimaanPelunasanSalesDto>(sql, dp);
            }
        }

        public IEnumerable<PenerimaanPelunasanSalesLunasSourceDto> ListPiutangLunasSource(Periode filter)
        {
            const string sql = @"
            SELECT
                aa.PiutangId,
                CAST(aa.LunasDate AS DATE) AS LunasDate,
                aa.JenisLunas,
                aa.Nilai,
                ISNULL(cc.SalesPersonId, '') AS SalesPersonId,
                ISNULL(cc.SalesPersonName, '') AS SalesName
            FROM BTR_PiutangLunas aa
                LEFT JOIN BTR_Faktur bb ON aa.PiutangId = bb.FakturId
                LEFT JOIN BTR_SalesPerson cc ON bb.SalesPersonId = cc.SalesPersonId
            WHERE aa.LunasDate BETWEEN @Tgl1 AND @Tgl2
            ORDER BY aa.LunasDate, cc.SalesPersonId, aa.PiutangId";

            var dp = new DynamicParameters();
            dp.AddParam("@Tgl1", filter.Tgl1, SqlDbType.DateTime);
            dp.AddParam("@Tgl2", filter.Tgl2, SqlDbType.DateTime);

            using (var conn = new SqlConnection(ConnStringHelper.Get(_opt)))
            {
                return conn.Read<PenerimaanPelunasanSalesLunasSourceDto>(sql, dp);
            }
        }

        public IEnumerable<PenerimaanPelunasanSalesElementDto> ListPiutangElementTotals(IEnumerable<string> piutangIds)
        {
            var ids = piutangIds?.Where(id => !string.IsNullOrEmpty(id)).Distinct().ToList()
                ?? new List<string>();
            if (ids.Count == 0)
                return Enumerable.Empty<PenerimaanPelunasanSalesElementDto>();

            const string sql = @"
            SELECT PiutangId,
                SUM(CASE WHEN ElementName = 'Retur' THEN NilaiPlus - NilaiMinus ELSE 0 END) AS Retur,
                SUM(CASE WHEN ElementName = 'Potongan' THEN NilaiPlus - NilaiMinus ELSE 0 END) AS Potongan,
                SUM(CASE WHEN ElementName IN ('Materai','Admin') THEN NilaiPlus - NilaiMinus ELSE 0 END) AS MateraiAdmin
            FROM BTR_PiutangElement
            WHERE PiutangId IN @PiutangIds
            GROUP BY PiutangId";

            using (var conn = new SqlConnection(ConnStringHelper.Get(_opt)))
            {
                var dp = new DynamicParameters();
                dp.Add("@PiutangIds", ids);
                return conn.Read<PenerimaanPelunasanSalesElementDto>(sql, dp);
            }
        }
    }
}
