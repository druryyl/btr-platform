using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using btr.application.SalesContext.SalesOmzetAgg.Contracts;
using btr.domain.SalesContext.SalesOmzetAgg;
using btr.infrastructure.Helpers;
using btr.nuna.Domain;
using btr.nuna.Infrastructure;
using Dapper;
using Microsoft.Extensions.Options;

namespace btr.infrastructure.SalesContext.SalesOmzetAgg
{
    public class SalesOmzetEntityDal : ISalesOmzetEntityDal
    {
        private const string SelectColumns = @"
                SalesOmzetId, OrderId, FakturId, SaleKind,
                SalesDate, OmzetDate,
                SalesPersonName, OrderDate, OrderTotal,
                FakturCode, FakturDate, FakturTotal,
                CustomerName, Code, Alamat, OmzetStatus,
                CreatedAt, LastReconciledAt";

        private readonly DatabaseOptions _opt;

        public SalesOmzetEntityDal(IOptions<DatabaseOptions> opt)
        {
            _opt = opt.Value;
        }

        public void Insert(SalesOmzetModel model)
        {
            const string sql = @"
            INSERT INTO BTR_SalesOmzet(
                SalesOmzetId, OrderId, FakturId, SaleKind,
                SalesDate, OmzetDate,
                SalesPersonName, OrderDate, OrderTotal,
                FakturCode, FakturDate, FakturTotal,
                CustomerName, Code, Alamat, OmzetStatus,
                CreatedAt, LastReconciledAt)
            VALUES (
                @SalesOmzetId, @OrderId, @FakturId, @SaleKind,
                @SalesDate, @OmzetDate,
                @SalesPersonName, @OrderDate, @OrderTotal,
                @FakturCode, @FakturDate, @FakturTotal,
                @CustomerName, @Code, @Alamat, @OmzetStatus,
                @CreatedAt, @LastReconciledAt)";

            using (var conn = new SqlConnection(ConnStringHelper.Get(_opt)))
            {
                conn.Execute(sql, CreateParameters(model));
            }
        }

        public void Update(SalesOmzetModel model)
        {
            const string sql = @"
            UPDATE BTR_SalesOmzet
            SET
                OrderId = @OrderId,
                FakturId = @FakturId,
                SaleKind = @SaleKind,
                SalesDate = @SalesDate,
                OmzetDate = @OmzetDate,
                SalesPersonName = @SalesPersonName,
                OrderDate = @OrderDate,
                OrderTotal = @OrderTotal,
                FakturCode = @FakturCode,
                FakturDate = @FakturDate,
                FakturTotal = @FakturTotal,
                CustomerName = @CustomerName,
                Code = @Code,
                Alamat = @Alamat,
                OmzetStatus = @OmzetStatus,
                CreatedAt = @CreatedAt,
                LastReconciledAt = @LastReconciledAt
            WHERE
                SalesOmzetId = @SalesOmzetId";

            using (var conn = new SqlConnection(ConnStringHelper.Get(_opt)))
            {
                conn.Execute(sql, CreateParameters(model));
            }
        }

        public SalesOmzetModel GetData(ISalesOmzetKey key)
        {
            var sql = $@"
            SELECT {SelectColumns}
            FROM BTR_SalesOmzet
            WHERE SalesOmzetId = @SalesOmzetId";

            var dp = new DynamicParameters();
            dp.AddParam("@SalesOmzetId", key.SalesOmzetId, SqlDbType.VarChar);

            using (var conn = new SqlConnection(ConnStringHelper.Get(_opt)))
            {
                return conn.ReadSingle<SalesOmzetModel>(sql, dp);
            }
        }

        public SalesOmzetModel GetByOrderId(string orderId)
        {
            var sql = $@"
            SELECT {SelectColumns}
            FROM BTR_SalesOmzet
            WHERE OrderId = @OrderId AND OrderId <> ''";

            var dp = new DynamicParameters();
            dp.AddParam("@OrderId", orderId, SqlDbType.VarChar);

            using (var conn = new SqlConnection(ConnStringHelper.Get(_opt)))
            {
                return conn.ReadSingle<SalesOmzetModel>(sql, dp);
            }
        }

        public SalesOmzetModel GetByFakturId(string fakturId)
        {
            var sql = $@"
            SELECT {SelectColumns}
            FROM BTR_SalesOmzet
            WHERE FakturId = @FakturId AND FakturId <> ''";

            var dp = new DynamicParameters();
            dp.AddParam("@FakturId", fakturId, SqlDbType.VarChar);

            using (var conn = new SqlConnection(ConnStringHelper.Get(_opt)))
            {
                return conn.ReadSingle<SalesOmzetModel>(sql, dp);
            }
        }

        public IEnumerable<SalesOmzetModel> ListForReconcileScope(Periode periode)
        {
            var sql = $@"
            SELECT {SelectColumns}
            FROM BTR_SalesOmzet
            WHERE SalesDate BETWEEN @Tgl1 AND @Tgl2
               OR OmzetDate BETWEEN @Tgl1 AND @Tgl2
               OR OrderDate BETWEEN @Tgl1 AND @Tgl2
               OR FakturDate BETWEEN @Tgl1 AND @Tgl2";

            var dp = new DynamicParameters();
            dp.AddParam("@Tgl1", periode.Tgl1, SqlDbType.DateTime);
            dp.AddParam("@Tgl2", periode.Tgl2, SqlDbType.DateTime);

            using (var conn = new SqlConnection(ConnStringHelper.Get(_opt)))
            {
                return conn.Read<SalesOmzetModel>(sql, dp);
            }
        }

        public IEnumerable<SalesOmzetModel> ListAll()
        {
            var sql = $@"
            SELECT {SelectColumns}
            FROM BTR_SalesOmzet";

            using (var conn = new SqlConnection(ConnStringHelper.Get(_opt)))
            {
                return conn.Read<SalesOmzetModel>(sql);
            }
        }

        private static DynamicParameters CreateParameters(SalesOmzetModel model)
        {
            var dp = new DynamicParameters();
            dp.AddParam("@SalesOmzetId", model.SalesOmzetId, SqlDbType.VarChar);
            dp.AddParam("@OrderId", model.OrderId, SqlDbType.VarChar);
            dp.AddParam("@FakturId", model.FakturId, SqlDbType.VarChar);
            dp.AddParam("@SaleKind", model.SaleKind.ToString(), SqlDbType.VarChar);
            dp.AddParam("@SalesDate", model.SalesDate, SqlDbType.DateTime);
            dp.AddParam("@OmzetDate", model.OmzetDate, SqlDbType.DateTime);
            dp.AddParam("@SalesPersonName", model.SalesPersonName, SqlDbType.VarChar);
            dp.AddParam("@OrderDate", model.OrderDate, SqlDbType.DateTime);
            dp.AddParam("@OrderTotal", model.OrderTotal, SqlDbType.Decimal);
            dp.AddParam("@FakturCode", model.FakturCode, SqlDbType.VarChar);
            dp.AddParam("@FakturDate", model.FakturDate, SqlDbType.DateTime);
            dp.AddParam("@FakturTotal", model.FakturTotal, SqlDbType.Decimal);
            dp.AddParam("@CustomerName", model.CustomerName, SqlDbType.VarChar);
            dp.AddParam("@Code", model.Code, SqlDbType.VarChar);
            dp.AddParam("@Alamat", model.Alamat, SqlDbType.VarChar);
            dp.AddParam("@OmzetStatus", model.OmzetStatus.ToString(), SqlDbType.VarChar);
            dp.AddParam("@CreatedAt", model.CreatedAt, SqlDbType.DateTime);
            dp.AddParam("@LastReconciledAt", model.LastReconciledAt, SqlDbType.DateTime);
            return dp;
        }
    }
}
