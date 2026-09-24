using Dapper;
using j07_btrade_sync.Model;
using Nuna.Lib.DataAccessHelper;
using Nuna.Lib.ValidationHelper;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;

namespace j07_btrade_sync.Repository
{
    //  Stages downloaded Return Orders into the Main Office BTR_ReturnOrder
    //  (Arch §4.3, §8.2, I-RO-07). Staging only: numbering (ReturnOrderNo) is
    //  owned by ImportReturnOrderCommand (S1.5/S3.4, IR-RO-07), so Insert
    //  writes an empty number and Update never overwrites it. TD-15 operator
    //  attribution (CreatedBy) is stamped at Insert from the relayed
    //  SubmittedBy, because this staging insert creates the row.
    public class ReturnOrderDal
    {
        private const string StagedStatus = "Synced";

        //  TD-15 — the staged row is created here, so the office audit identity
        //  (CreatedBy) is written here. The Cloud-resolved operator UserId
        //  (SubmittedBy) is used when present; a legacy row without SubmittedBy
        //  falls back to fallbackCreatedBy, the existing sync service-account
        //  identity, preserving the pre-relay audit behavior. Update never
        //  rewrites CreatedBy, so a re-download preserves the captured
        //  attribution.
        public void Insert(ReturnOrderModel model, string fallbackCreatedBy = "")
        {
            const string sql = @"
                INSERT INTO BTR_ReturnOrder(
                    ReturnOrderId, ReturnOrderNo, ReturnOrderDate, WarehouseCode,
                    CustomerId, SalesPersonId, DriverId, Note, Status, CreatedBy)
                VALUES (
                    @ReturnOrderId, @ReturnOrderNo, @ReturnOrderDate, @WarehouseCode,
                    @CustomerId, @SalesPersonId, @DriverId, @Note, @Status, @CreatedBy)";

            var dp = new DynamicParameters();
            dp.AddParam("@ReturnOrderId", model.ReturnOrderId, SqlDbType.VarChar);
            dp.AddParam("@ReturnOrderNo", string.Empty, SqlDbType.VarChar);
            dp.AddParam("@ReturnOrderDate", ToDate(model.ReturnOrderDate), SqlDbType.DateTime);
            dp.AddParam("@WarehouseCode", model.WarehouseCode ?? string.Empty, SqlDbType.VarChar);
            dp.AddParam("@CustomerId", model.CustomerId ?? string.Empty, SqlDbType.VarChar);
            dp.AddParam("@SalesPersonId", model.SalesPersonId ?? string.Empty, SqlDbType.VarChar);
            dp.AddParam("@DriverId", model.DriverId ?? string.Empty, SqlDbType.VarChar);
            dp.AddParam("@Note", model.Note ?? string.Empty, SqlDbType.VarChar);
            dp.AddParam("@Status", StagedStatus, SqlDbType.VarChar);

            var createdBy = string.IsNullOrWhiteSpace(model.SubmittedBy)
                ? (fallbackCreatedBy ?? string.Empty)
                : model.SubmittedBy;
            dp.AddParam("@CreatedBy", createdBy, SqlDbType.VarChar);

            using (var conn = new SqlConnection(ConnStringHelper.Get()))
            {
                conn.Execute(sql, dp);
            }
        }

        public void Update(ReturnOrderModel model)
        {
            //  ReturnOrderNo is intentionally excluded: the office import command
            //  owns numbering (IR-RO-07); re-download must never wipe it (INV-11).
            const string sql = @"
            UPDATE
                BTR_ReturnOrder
            SET
                ReturnOrderDate = @ReturnOrderDate,
                WarehouseCode = @WarehouseCode,
                CustomerId = @CustomerId,
                SalesPersonId = @SalesPersonId,
                DriverId = @DriverId,
                Note = @Note,
                Status = @Status
            WHERE
                ReturnOrderId = @ReturnOrderId";

            var dp = new DynamicParameters();
            dp.AddParam("@ReturnOrderId", model.ReturnOrderId, SqlDbType.VarChar);
            dp.AddParam("@ReturnOrderDate", ToDate(model.ReturnOrderDate), SqlDbType.DateTime);
            dp.AddParam("@WarehouseCode", model.WarehouseCode ?? string.Empty, SqlDbType.VarChar);
            dp.AddParam("@CustomerId", model.CustomerId ?? string.Empty, SqlDbType.VarChar);
            dp.AddParam("@SalesPersonId", model.SalesPersonId ?? string.Empty, SqlDbType.VarChar);
            dp.AddParam("@DriverId", model.DriverId ?? string.Empty, SqlDbType.VarChar);
            dp.AddParam("@Note", model.Note ?? string.Empty, SqlDbType.VarChar);
            dp.AddParam("@Status", StagedStatus, SqlDbType.VarChar);

            using (var conn = new SqlConnection(ConnStringHelper.Get()))
            {
                conn.Execute(sql, dp);
            }
        }

        public ReturnOrderModel GetData(IReturnOrderKey key)
        {
            const string sql = @"
            SELECT
                ReturnOrderId,
                CONVERT(VARCHAR(10), ReturnOrderDate, 120) AS ReturnOrderDate,
                WarehouseCode, CustomerId, SalesPersonId, DriverId, Note,
                Status AS StatusSync,
                '' AS CustomerName, '' AS SalesPersonName, '' AS DriverName
            FROM
                BTR_ReturnOrder
            WHERE
                ReturnOrderId = @ReturnOrderId";

            var dp = new DynamicParameters();
            dp.AddParam("@ReturnOrderId", key.ReturnOrderId, SqlDbType.VarChar);

            using (var conn = new SqlConnection(ConnStringHelper.Get()))
            {
                return conn.ReadSingle<ReturnOrderModel>(sql, dp);
            }
        }

        private static DateTime ToDate(string value)
        {
            if (DateTime.TryParseExact(value, "yyyy-MM-dd",
                CultureInfo.InvariantCulture, DateTimeStyles.None, out var result))
                return result;
            if (DateTime.TryParse(value, out result))
                return result;
            return new DateTime(3000, 1, 1);
        }
    }
}
