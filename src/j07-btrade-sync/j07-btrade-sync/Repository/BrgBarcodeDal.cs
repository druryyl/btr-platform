using Dapper;
using j07_btrade_sync.Model;
using Nuna.Lib.DataAccessHelper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace j07_btrade_sync.Repository
{
    public class BrgBarcodeDal
    {
        public IEnumerable<BrgBarcodeType> ListChanged(byte[] watermark)
        {
            const string sql = @"
                SELECT
                    aa.BrgBarcodeId, aa.BarcodeValue, aa.BrgId,
                    ISNULL(bb.BrgCode, '') BrgCode,
                    ISNULL(bb.BrgName, '') BrgName,
                    aa.Satuan, aa.IsAktif, aa.RowVer
                FROM
                    BTR_BrgBarcode aa
                    LEFT JOIN BTR_Brg bb ON aa.BrgId = bb.BrgId
                WHERE
                    (@watermark IS NULL OR aa.RowVer > @watermark)
                ORDER BY
                    aa.RowVer ";

            var dp = new DynamicParameters();
            dp.AddParam("@watermark", watermark, SqlDbType.Image);

            using (var conn = new SqlConnection(ConnStringHelper.Get()))
            {
                var result = conn.Query<BrgBarcodeType>(sql, dp).ToList();
                return result;
            }
        }
    }
}
