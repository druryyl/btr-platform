using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using btr.application.InventoryContext.ReturnOrderAgg.Contracts;
using btr.domain.InventoryContext.ReturnOrderAgg;
using btr.infrastructure.Helpers;
using btr.nuna.Infrastructure;
using Dapper;
using Microsoft.Extensions.Options;

namespace btr.infrastructure.InventoryContext.ReturnOrderAgg
{
    public class ReturnOrderDal : IReturnOrderDal
    {
        private readonly DatabaseOptions _opt;

        public ReturnOrderDal(IOptions<DatabaseOptions> opt)
        {
            _opt = opt.Value;
        }

        public void Insert(ReturnOrderModel model)
        {
            const string sql = @"
                INSERT INTO 
                    BTR_ReturnOrder (
                        ReturnOrderId, ReturnOrderNo, ReturnOrderDate, WarehouseCode,
                        CustomerId, SalesPersonId, DriverId, Note, Status,
                        CreatedBy, CreatedDate, ModifiedBy, ModifiedDate
                    )
                VALUES (
                        @ReturnOrderId, @ReturnOrderNo, @ReturnOrderDate, @WarehouseCode,
                        @CustomerId, @SalesPersonId, @DriverId, @Note, @Status,
                        @CreatedBy, @CreatedDate, @ModifiedBy, @ModifiedDate
                    )";

            var dp = new DynamicParameters();
            dp.AddParam("@ReturnOrderId", model.ReturnOrderId, SqlDbType.VarChar);
            dp.AddParam("@ReturnOrderNo", model.ReturnOrderNo, SqlDbType.VarChar);
            dp.AddParam("@ReturnOrderDate", model.ReturnOrderDate, SqlDbType.DateTime);
            dp.AddParam("@WarehouseCode", model.WarehouseCode, SqlDbType.VarChar);
            dp.AddParam("@CustomerId", model.CustomerId, SqlDbType.VarChar);
            dp.AddParam("@SalesPersonId", model.SalesPersonId, SqlDbType.VarChar);
            dp.AddParam("@DriverId", model.DriverId, SqlDbType.VarChar);
            dp.AddParam("@Note", model.Note, SqlDbType.VarChar);
            dp.AddParam("@Status", model.Status, SqlDbType.VarChar);
            dp.AddParam("@CreatedBy", model.CreatedBy, SqlDbType.VarChar);
            dp.AddParam("@CreatedDate", model.CreatedDate, SqlDbType.DateTime);
            dp.AddParam("@ModifiedBy", model.ModifiedBy, SqlDbType.VarChar);
            dp.AddParam("@ModifiedDate", model.ModifiedDate, SqlDbType.DateTime);

            using (var conn = new SqlConnection(ConnStringHelper.Get(_opt)))
            {
                conn.Execute(sql, dp);
            }
        }

        public void Update(ReturnOrderModel model)
        {
            const string sql = @"
                UPDATE 
                    BTR_ReturnOrder 
                SET 
                    ReturnOrderNo = @ReturnOrderNo,
                    ReturnOrderDate = @ReturnOrderDate,
                    WarehouseCode = @WarehouseCode,
                    CustomerId = @CustomerId,
                    SalesPersonId = @SalesPersonId,
                    DriverId = @DriverId,
                    Note = @Note,
                    Status = @Status,
                    ModifiedBy = @ModifiedBy,
                    ModifiedDate = @ModifiedDate
                WHERE 
                    ReturnOrderId = @ReturnOrderId";

            var dp = new DynamicParameters();
            dp.AddParam("@ReturnOrderId", model.ReturnOrderId, SqlDbType.VarChar);
            dp.AddParam("@ReturnOrderNo", model.ReturnOrderNo, SqlDbType.VarChar);
            dp.AddParam("@ReturnOrderDate", model.ReturnOrderDate, SqlDbType.DateTime);
            dp.AddParam("@WarehouseCode", model.WarehouseCode, SqlDbType.VarChar);
            dp.AddParam("@CustomerId", model.CustomerId, SqlDbType.VarChar);
            dp.AddParam("@SalesPersonId", model.SalesPersonId, SqlDbType.VarChar);
            dp.AddParam("@DriverId", model.DriverId, SqlDbType.VarChar);
            dp.AddParam("@Note", model.Note, SqlDbType.VarChar);
            dp.AddParam("@Status", model.Status, SqlDbType.VarChar);
            dp.AddParam("@ModifiedBy", model.ModifiedBy, SqlDbType.VarChar);
            dp.AddParam("@ModifiedDate", model.ModifiedDate, SqlDbType.DateTime);

            using (var conn = new SqlConnection(ConnStringHelper.Get(_opt)))
            {
                conn.Execute(sql, dp);
            }
        }

        public ReturnOrderModel GetData(IReturnOrderKey key)
        {
            return GetByReturnOrderId(key);
        }

        public ReturnOrderModel GetByReturnOrderId(IReturnOrderKey key)
        {
            const string sql = @"
                SELECT 
                    aa.ReturnOrderId, aa.ReturnOrderNo, aa.ReturnOrderDate, aa.WarehouseCode,
                    aa.CustomerId, aa.SalesPersonId, aa.DriverId, aa.Note, aa.Status,
                    aa.CreatedBy, aa.CreatedDate, aa.ModifiedBy, aa.ModifiedDate,
                    ISNULL(bb.CustomerName, '') AS CustomerName,
                    ISNULL(cc.SalesPersonName, '') AS SalesPersonName,
                    ISNULL(dd.DriverName, '') AS DriverName
                FROM 
                    BTR_ReturnOrder aa
                    LEFT JOIN BTR_Customer bb ON aa.CustomerId = bb.CustomerId
                    LEFT JOIN BTR_SalesPerson cc ON aa.SalesPersonId = cc.SalesPersonId
                    LEFT JOIN BTR_Driver dd ON aa.DriverId = dd.DriverId
                WHERE 
                    aa.ReturnOrderId = @ReturnOrderId";

            var dp = new DynamicParameters();
            dp.AddParam("@ReturnOrderId", key.ReturnOrderId, SqlDbType.VarChar);

            using (var conn = new SqlConnection(ConnStringHelper.Get(_opt)))
            {
                return conn.ReadSingle<ReturnOrderModel>(sql, dp);
            }
        }

        public IEnumerable<ReturnOrderModel> ListData()
        {
            const string sql = @"
                SELECT 
                    aa.ReturnOrderId, aa.ReturnOrderNo, aa.ReturnOrderDate, aa.WarehouseCode,
                    aa.CustomerId, aa.SalesPersonId, aa.DriverId, aa.Note, aa.Status,
                    aa.CreatedBy, aa.CreatedDate, aa.ModifiedBy, aa.ModifiedDate,
                    ISNULL(bb.CustomerName, '') AS CustomerName,
                    ISNULL(cc.SalesPersonName, '') AS SalesPersonName,
                    ISNULL(dd.DriverName, '') AS DriverName
                FROM 
                    BTR_ReturnOrder aa
                    LEFT JOIN BTR_Customer bb ON aa.CustomerId = bb.CustomerId
                    LEFT JOIN BTR_SalesPerson cc ON aa.SalesPersonId = cc.SalesPersonId
                    LEFT JOIN BTR_Driver dd ON aa.DriverId = dd.DriverId
                ORDER BY 
                    aa.ReturnOrderDate DESC, aa.ReturnOrderId DESC";

            using (var conn = new SqlConnection(ConnStringHelper.Get(_opt)))
            {
                return conn.Read<ReturnOrderModel>(sql);
            }
        }

        public IEnumerable<ReturnOrderModel> ListByStatus(string status)
        {
            const string sql = @"
                SELECT 
                    aa.ReturnOrderId, aa.ReturnOrderNo, aa.ReturnOrderDate, aa.WarehouseCode,
                    aa.CustomerId, aa.SalesPersonId, aa.DriverId, aa.Note, aa.Status,
                    aa.CreatedBy, aa.CreatedDate, aa.ModifiedBy, aa.ModifiedDate,
                    ISNULL(bb.CustomerName, '') AS CustomerName,
                    ISNULL(cc.SalesPersonName, '') AS SalesPersonName,
                    ISNULL(dd.DriverName, '') AS DriverName
                FROM 
                    BTR_ReturnOrder aa
                    LEFT JOIN BTR_Customer bb ON aa.CustomerId = bb.CustomerId
                    LEFT JOIN BTR_SalesPerson cc ON aa.SalesPersonId = cc.SalesPersonId
                    LEFT JOIN BTR_Driver dd ON aa.DriverId = dd.DriverId
                WHERE 
                    aa.Status = @Status
                ORDER BY 
                    aa.ReturnOrderDate DESC, aa.ReturnOrderId DESC";

            var dp = new DynamicParameters();
            dp.AddParam("@Status", status, SqlDbType.VarChar);

            using (var conn = new SqlConnection(ConnStringHelper.Get(_opt)))
            {
                return conn.Read<ReturnOrderModel>(sql, dp);
            }
        }

        public bool Exists(IReturnOrderKey key)
        {
            const string sql = @"
                SELECT 
                    COUNT(1)
                FROM 
                    BTR_ReturnOrder
                WHERE 
                    ReturnOrderId = @ReturnOrderId";

            var dp = new DynamicParameters();
            dp.AddParam("@ReturnOrderId", key.ReturnOrderId, SqlDbType.VarChar);

            using (var conn = new SqlConnection(ConnStringHelper.Get(_opt)))
            {
                return conn.ExecuteScalar<int>(sql, dp) > 0;
            }
        }
    }
}
