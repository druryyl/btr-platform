using btr.application.SalesContext.FakturPerCustomerRpt;
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
using System.Threading;
using System.Threading.Tasks;

namespace btr.infrastructure.SalesContext.FakturPerCustomerRpt
{
    public class FakturPerCustomerDal : IFakturPerCustomerDal
    {
        private readonly DatabaseOptions _opt;

        public FakturPerCustomerDal(IOptions<DatabaseOptions> opt)
        {
            _opt = opt.Value;
        }

        private const string SqlQuery = @"
            SELECT
                ISNULL(bb.FakturCode, '') AS FakturCode,
                ISNULL(bb.FakturDate, '3000-01-01') AS FakturDate,
                ISNULL(bb.DueDate, '3000-01-01') AS DueDate,
                ISNULL(gg.SupplierCode, '') AS SupplierCode,
                ISNULL(gg.SupplierName, '') AS SupplierName,
                ISNULL(hh.KategoriName, '') AS KategoriName,
                ISNULL(cc.BrgCode, '') AS BrgCode,
                ISNULL(cc.BrgName, '') AS BrgName,
                ISNULL(dd.CustomerCode, '') AS CustomerCode,
                ISNULL(dd.CustomerName, '') AS CustomerName, 
                ISNULL(dd.Address1, '') AS CustomerAddress,
                ISNULL(dd.Kota, '') AS CustomerKota,
                ISNULL(ii.WilayahName, '') AS WilayahName,
                ISNULL(ee.SalesPersonName, '') AS SalesPersonName,
                aa.QtyBesar,
                aa.SatBesar,
                aa.HrgSatBesar,
                aa.QtyKecil,
                aa.SatKecil,
                aa.HrgSatKecil,
                aa.QtyBonus,
                aa.QtyPotStok,
                aa.SubTotal,
                ISNULL(ff.DiscProsen1, 0) AS DiscProsen1,
                ISNULL(ff.DiscProsen2, 0) AS DiscProsen2,
                ISNULL(ff.DiscProsen3, 0) AS DiscProsen3,
                ISNULL(ff.DiscProsen4, 0) AS DiscProsen4,
                ISNULL(aa.DiscRp, 0) AS TotalDisc,
                aa.SubTotal - ISNULL(aa.DiscRp, 0) AS TotalSebelumTax,
                aa.PpnRp,
                aa.Total,
                ISNULL(jj.StatusFaktur, 0) AS StatusFaktur,
                ISNULL(kk.KlasifikasiName, '') AS KlasifikasiName
            FROM
                BTR_Faktur bb
                INNER JOIN BTR_FakturItem aa ON bb.FakturId = aa.FakturId
                LEFT JOIN BTR_Brg cc ON aa.BrgId = cc.BrgId
                LEFT JOIN BTR_Customer dd ON bb.CustomerId = dd.CustomerId
                LEFT JOIN BTR_SalesPerson ee ON bb.SalesPersonId = ee.SalesPersonId
                LEFT JOIN (
                    SELECT 
                        fd.FakturId,
                        fd.FakturItemId,
                        MAX(CASE WHEN fd.NoUrut = 1 THEN fd.DiscProsen ELSE 0 END) AS DiscProsen1,
                        MAX(CASE WHEN fd.NoUrut = 2 THEN fd.DiscProsen ELSE 0 END) AS DiscProsen2,
                        MAX(CASE WHEN fd.NoUrut = 3 THEN fd.DiscProsen ELSE 0 END) AS DiscProsen3,
                        MAX(CASE WHEN fd.NoUrut = 4 THEN fd.DiscProsen ELSE 0 END) AS DiscProsen4
                    FROM BTR_FakturDiscount fd
                    INNER JOIN BTR_Faktur f ON fd.FakturId = f.FakturId
                    WHERE f.FakturDate BETWEEN @Tgl1 AND @Tgl2
                      AND f.VoidDate = '3000-01-01'
                      AND fd.NoUrut BETWEEN 1 AND 4
                    GROUP BY fd.FakturId, fd.FakturItemId
                ) ff ON aa.FakturId = ff.FakturId AND aa.FakturItemId = ff.FakturItemId
                LEFT JOIN BTR_Supplier gg ON cc.SupplierId = gg.SupplierId
                LEFT JOIN BTR_Kategori hh ON cc.KategoriId = hh.KategoriId
                LEFT JOIN BTR_Wilayah ii ON dd.WilayahId = ii.WilayahId
                LEFT JOIN BTR_FakturControlStatus jj ON bb.FakturId = jj.FakturId AND jj.StatusFaktur = 2
                LEFT JOIN BTR_Klasifikasi kk ON dd.KlasifikasiId = kk.KlasifikasiId
            WHERE
                bb.FakturDate BETWEEN @Tgl1 AND @Tgl2 
                AND bb.VoidDate = '3000-01-01'";

        public IEnumerable<FakturPerCustomerView> ListData(Periode filter)
        {
            var dp = new DynamicParameters();
            dp.AddParam("@Tgl1", filter.Tgl1, SqlDbType.DateTime);
            dp.AddParam("@Tgl2", filter.Tgl2, SqlDbType.DateTime);

            using (var cn = new SqlConnection(ConnStringHelper.Get(_opt)))
            {
                var result = cn.Query<FakturPerCustomerView>(SqlQuery, dp, commandTimeout: 120);
                return result ?? new List<FakturPerCustomerView>();
            }
        }

        public async Task<IEnumerable<FakturPerCustomerView>> ListDataAsync(Periode filter, CancellationToken cancellationToken = default)
        {
            var dp = new DynamicParameters();
            dp.AddParam("@Tgl1", filter.Tgl1, SqlDbType.DateTime);
            dp.AddParam("@Tgl2", filter.Tgl2, SqlDbType.DateTime);

            using (var cn = new SqlConnection(ConnStringHelper.Get(_opt)))
            {
                await cn.OpenAsync(cancellationToken);
                var cmd = new CommandDefinition(SqlQuery, dp, commandTimeout: 120, cancellationToken: cancellationToken);
                var result = await cn.QueryAsync<FakturPerCustomerView>(cmd);
                return result ?? new List<FakturPerCustomerView>();
            }
        }
    }
}
