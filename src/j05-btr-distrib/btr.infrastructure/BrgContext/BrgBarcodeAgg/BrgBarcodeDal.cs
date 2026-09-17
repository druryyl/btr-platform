using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using btr.application.BrgContext.BrgBarcodeAgg;
using btr.domain.BrgContext.BrgAgg;
using btr.domain.BrgContext.BrgBarcodeAgg;
using btr.infrastructure.Helpers;
using btr.nuna.Infrastructure;
using Dapper;
using Microsoft.Extensions.Options;

namespace btr.infrastructure.BrgContext.BrgBarcodeAgg
{
    public class BrgBarcodeDal : IBrgBarcodeDal
    {
        private readonly DatabaseOptions _opt;

        public BrgBarcodeDal(IOptions<DatabaseOptions> opt)
        {
            _opt = opt.Value;
        }

        public void Insert(BrgBarcodeModel model)
        {
            const string sql = @"
                INSERT INTO BTR_BrgBarcode(
                    BrgBarcodeId, BarcodeValue, BrgId, Satuan, IsAktif,
                    CreatedBy, CreatedDate, ModifiedBy, ModifiedDate)
                VALUES (
                    @BrgBarcodeId, @BarcodeValue, @BrgId, @Satuan, @IsAktif,
                    @CreatedBy, @CreatedDate, @ModifiedBy, @ModifiedDate)";

            var dp = new DynamicParameters();
            dp.AddParam("@BrgBarcodeId", model.BrgBarcodeId, SqlDbType.VarChar);
            dp.AddParam("@BarcodeValue", model.BarcodeValue, SqlDbType.VarChar);
            dp.AddParam("@BrgId", model.BrgId, SqlDbType.VarChar);
            dp.AddParam("@Satuan", model.Satuan ?? string.Empty, SqlDbType.VarChar);
            dp.AddParam("@IsAktif", model.IsAktif, SqlDbType.Bit);
            dp.AddParam("@CreatedBy", model.CreatedBy ?? string.Empty, SqlDbType.VarChar);
            dp.AddParam("@CreatedDate", model.CreatedDate, SqlDbType.DateTime);
            dp.AddParam("@ModifiedBy", model.ModifiedBy ?? string.Empty, SqlDbType.VarChar);
            dp.AddParam("@ModifiedDate", model.ModifiedDate, SqlDbType.DateTime);

            using (var conn = new SqlConnection(ConnStringHelper.Get(_opt)))
            {
                conn.Execute(sql, dp);
            }
        }

        public void Update(BrgBarcodeModel model)
        {
            const string sql = @"
                UPDATE
                    BTR_BrgBarcode
                SET
                    BarcodeValue = @BarcodeValue,
                    BrgId = @BrgId,
                    Satuan = @Satuan,
                    IsAktif = @IsAktif,
                    ModifiedBy = @ModifiedBy,
                    ModifiedDate = @ModifiedDate
                WHERE
                    BrgBarcodeId = @BrgBarcodeId ";

            var dp = new DynamicParameters();
            dp.AddParam("@BrgBarcodeId", model.BrgBarcodeId, SqlDbType.VarChar);
            dp.AddParam("@BarcodeValue", model.BarcodeValue, SqlDbType.VarChar);
            dp.AddParam("@BrgId", model.BrgId, SqlDbType.VarChar);
            dp.AddParam("@Satuan", model.Satuan ?? string.Empty, SqlDbType.VarChar);
            dp.AddParam("@IsAktif", model.IsAktif, SqlDbType.Bit);
            dp.AddParam("@ModifiedBy", model.ModifiedBy ?? string.Empty, SqlDbType.VarChar);
            dp.AddParam("@ModifiedDate", model.ModifiedDate, SqlDbType.DateTime);

            using (var conn = new SqlConnection(ConnStringHelper.Get(_opt)))
            {
                conn.Execute(sql, dp);
            }
        }

        public BrgBarcodeModel GetData(IBrgBarcodeKey key)
        {
            const string sql = @"
                SELECT
                    aa.BrgBarcodeId, aa.BarcodeValue, aa.BrgId,
                    ISNULL(bb.BrgCode, '') BrgCode,
                    ISNULL(bb.BrgName, '') BrgName,
                    aa.Satuan, aa.IsAktif,
                    aa.CreatedBy, aa.CreatedDate, aa.ModifiedBy, aa.ModifiedDate
                FROM
                    BTR_BrgBarcode aa
                    LEFT JOIN BTR_Brg bb ON aa.BrgId = bb.BrgId
                WHERE
                    aa.BrgBarcodeId = @BrgBarcodeId ";

            var dp = new DynamicParameters();
            dp.AddParam("@BrgBarcodeId", key.BrgBarcodeId, SqlDbType.VarChar);

            using (var conn = new SqlConnection(ConnStringHelper.Get(_opt)))
            {
                return conn.ReadSingle<BrgBarcodeModel>(sql, dp);
            }
        }

        public IEnumerable<BrgBarcodeModel> ListData()
        {
            const string sql = @"
                SELECT
                    aa.BrgBarcodeId, aa.BarcodeValue, aa.BrgId,
                    ISNULL(bb.BrgCode, '') BrgCode,
                    ISNULL(bb.BrgName, '') BrgName,
                    aa.Satuan, aa.IsAktif,
                    aa.CreatedBy, aa.CreatedDate, aa.ModifiedBy, aa.ModifiedDate
                FROM
                    BTR_BrgBarcode aa
                    LEFT JOIN BTR_Brg bb ON aa.BrgId = bb.BrgId ";

            using (var conn = new SqlConnection(ConnStringHelper.Get(_opt)))
            {
                return conn.Read<BrgBarcodeModel>(sql);
            }
        }

        public BrgBarcodeModel GetByValue(string barcodeValueKey)
        {
            const string sql = @"
                SELECT
                    aa.BrgBarcodeId, aa.BarcodeValue, aa.BrgId,
                    ISNULL(bb.BrgCode, '') BrgCode,
                    ISNULL(bb.BrgName, '') BrgName,
                    aa.Satuan, aa.IsAktif,
                    aa.CreatedBy, aa.CreatedDate, aa.ModifiedBy, aa.ModifiedDate
                FROM
                    BTR_BrgBarcode aa
                    LEFT JOIN BTR_Brg bb ON aa.BrgId = bb.BrgId
                WHERE
                    aa.BarcodeValueKey = @BarcodeValueKey ";

            var dp = new DynamicParameters();
            dp.AddParam("@BarcodeValueKey", barcodeValueKey, SqlDbType.VarChar);

            using (var conn = new SqlConnection(ConnStringHelper.Get(_opt)))
            {
                return conn.ReadSingle<BrgBarcodeModel>(sql, dp);
            }
        }

        public IEnumerable<BrgBarcodeModel> ListByBrg(IBrgKey brg)
        {
            const string sql = @"
                SELECT
                    aa.BrgBarcodeId, aa.BarcodeValue, aa.BrgId,
                    ISNULL(bb.BrgCode, '') BrgCode,
                    ISNULL(bb.BrgName, '') BrgName,
                    aa.Satuan, aa.IsAktif,
                    aa.CreatedBy, aa.CreatedDate, aa.ModifiedBy, aa.ModifiedDate
                FROM
                    BTR_BrgBarcode aa
                    LEFT JOIN BTR_Brg bb ON aa.BrgId = bb.BrgId
                WHERE
                    aa.BrgId = @BrgId ";

            var dp = new DynamicParameters();
            dp.AddParam("@BrgId", brg.BrgId, SqlDbType.VarChar);

            using (var conn = new SqlConnection(ConnStringHelper.Get(_opt)))
            {
                return conn.Read<BrgBarcodeModel>(sql, dp);
            }
        }

        public IEnumerable<BrgBarcodeModel> ListChanged(byte[] watermark)
        {
            const string sql = @"
                SELECT
                    aa.BrgBarcodeId, aa.BarcodeValue, aa.BrgId,
                    ISNULL(bb.BrgCode, '') BrgCode,
                    ISNULL(bb.BrgName, '') BrgName,
                    aa.Satuan, aa.IsAktif,
                    aa.CreatedBy, aa.CreatedDate, aa.ModifiedBy, aa.ModifiedDate
                FROM
                    BTR_BrgBarcode aa
                    LEFT JOIN BTR_Brg bb ON aa.BrgId = bb.BrgId
                WHERE
                    (@watermark IS NULL OR aa.RowVer > @watermark)
                ORDER BY
                    aa.RowVer ";

            var dp = new DynamicParameters();
            dp.AddParam("@watermark", watermark, SqlDbType.Image);

            using (var conn = new SqlConnection(ConnStringHelper.Get(_opt)))
            {
                return conn.Read<BrgBarcodeModel>(sql, dp);
            }
        }

        public bool ExistsByValue(string barcodeValueKey)
        {
            const string sql = @"
                SELECT
                    COUNT(1)
                FROM
                    BTR_BrgBarcode
                WHERE
                    BarcodeValueKey = @BarcodeValueKey ";

            var dp = new DynamicParameters();
            dp.AddParam("@BarcodeValueKey", barcodeValueKey, SqlDbType.VarChar);

            using (var conn = new SqlConnection(ConnStringHelper.Get(_opt)))
            {
                return conn.ExecuteScalar<int>(sql, dp) > 0;
            }
        }
    }
}
