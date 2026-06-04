using Bilreg.Infrastructure.Shared.Helpers;
using BtrGudang.Domain.PackingOrderFeature;
using BtrGudang.Helper.Common;
using BtrGudang.Winform.Infrastructure;
using Dapper;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

namespace BtrGudang.Infrastructure.PackingOrderFeature
{
    public interface IPackingOrderDal :
        IInsert<PackingOrderDto>,
        IUpdate<PackingOrderDto>,
        IDelete<IPackingOrderKey>,
        IGetData<PackingOrderDto, IPackingOrderKey>,
        IListData<PackingOrderDto, Periode>
    {
        IEnumerable<PackingOrderView> ListDataView(Periode periode);
        IEnumerable<PackingOrderView> ListByDownloadTimestamp(DateTime downloadTimestamp);
        PackingOrderView GetLastDownloadByTimestamp();
    }

    public class PackingOrderDal : IPackingOrderDal
    {
        private readonly DatabaseOptions _opt;

        public PackingOrderDal(IOptions<DatabaseOptions> opt)
        {
            _opt = opt.Value;
        }

        public void Insert(PackingOrderDto dto)
        {
            const string sql = @"
                INSERT INTO BTRG_PackingOrder(
                    PackingOrderId, PackingOrderDate, 
                    CustomerId, CustomerCode, CustomerName, Alamat, NoTelp,
                    Latitude, Longitude, Accuracy,
                    FakturId, FakturCode, FakturDate, AdminName, GrandTotal,
                    DriverId, DriverName, DownloadTimestamp, OfficeCode, 
                    PrintLogId, Note)
                VALUES(
                    @PackingOrderId, @PackingOrderDate, 
                    @CustomerId, @CustomerCode, @CustomerName, @Alamat, @NoTelp,
                    @Latitude, @Longitude, @Accuracy,
                    @FakturId, @FakturCode, @FakturDate, @AdminName, @GrandTotal,
                    @DriverId, @DriverName, @DownloadTimestamp, @OfficeCode, 
                    @PrintLogId, @Note)
                ";
            var dp = new DynamicParameters();
            dp.AddParam("@PackingOrderId", dto.PackingOrderId, SqlDbType.VarChar);
            dp.AddParam("@PackingOrderDate", dto.PackingOrderDate, SqlDbType.DateTime);

            dp.AddParam("@CustomerId", dto.CustomerId, SqlDbType.VarChar);
            dp.AddParam("@CustomerCode", dto.CustomerCode, SqlDbType.VarChar);
            dp.AddParam("@CustomerName", dto.CustomerName, SqlDbType.VarChar);
            dp.AddParam("@Alamat", dto.Alamat, SqlDbType.VarChar);
            dp.AddParam("@NoTelp", dto.NoTelp, SqlDbType.VarChar);

            dp.AddParam("@Latitude", dto.Latitude, SqlDbType.Float);
            dp.AddParam("@Longitude", dto.Longitude, SqlDbType.Float);
            dp.AddParam("@Accuracy", dto.Accuracy, SqlDbType.Float);

            dp.AddParam("@FakturId", dto.FakturId, SqlDbType.VarChar);
            dp.AddParam("@FakturCode", dto.FakturCode, SqlDbType.VarChar);
            dp.AddParam("@FakturDate", dto.FakturDate, SqlDbType.DateTime);
            dp.AddParam("@AdminName", dto.AdminName, SqlDbType.VarChar);
            dp.AddParam("@GrandTotal", dto.GrandTotal, SqlDbType.Decimal);

            dp.AddParam("@DriverId", dto.DriverId, SqlDbType.VarChar);
            dp.AddParam("@DriverName", dto.DriverName, SqlDbType.VarChar);

            dp.AddParam("@DownloadTimestamp", dto.DownloadTimestamp, SqlDbType.DateTime);
            dp.AddParam("@OfficeCode", dto.OfficeCode, SqlDbType.VarChar);
            dp.AddParam("@PrintLogId", dto.PrintLogId, SqlDbType.VarChar);
            dp.AddParam("@Note", dto.Note, SqlDbType.VarChar);

            var connStr = ConnStringHelper.Get(_opt);
            using (var conn = new SqlConnection(connStr))
            {
                conn.Execute(sql, dp);
            }
        }

        public void Update(PackingOrderDto dto)
        {
            const string sql = @"
                UPDATE BTRG_PackingOrder
                SET
                    PackingOrderDate = @PackingOrderDate,
                    CustomerId = @CustomerId,
                    CustomerCode = @CustomerCode,
                    CustomerName = @CustomerName,
                    Alamat = @Alamat,
                    NoTelp = @NoTelp,

                    Latitude = @Latitude,
                    Longitude = @Longitude,
                    Accuracy = @Accuracy,

                    FakturId = @FakturId,
                    FakturCode = @FakturCode,
                    FakturDate = @FakturDate,
                    AdminName = @AdminName,
                    GrandTotal = @GrandTotal,

                    DriverId = @DriverId,
                    DriverName = @DriverName,
                    DownloadTimestamp = @DownloadTimestamp,
                    OfficeCode = @OfficeCode,
                    PrintLogId = @PrintLogId,
                    Note = @Note
                WHERE
                    PackingOrderId = @PackingOrderId
                ";

            var dp = new DynamicParameters();
            dp.AddParam("@PackingOrderId", dto.PackingOrderId, SqlDbType.VarChar);
            dp.AddParam("@PackingOrderDate", dto.PackingOrderDate, SqlDbType.DateTime);

            dp.AddParam("@CustomerId", dto.CustomerId, SqlDbType.VarChar);
            dp.AddParam("@CustomerCode", dto.CustomerCode, SqlDbType.VarChar);
            dp.AddParam("@CustomerName", dto.CustomerName, SqlDbType.VarChar);
            dp.AddParam("@Alamat", dto.Alamat, SqlDbType.VarChar);
            dp.AddParam("@NoTelp", dto.NoTelp, SqlDbType.VarChar);

            dp.AddParam("@Latitude", dto.Latitude, SqlDbType.Float);
            dp.AddParam("@Longitude", dto.Longitude, SqlDbType.Float);
            dp.AddParam("@Accuracy", dto.Accuracy, SqlDbType.Float);

            dp.AddParam("@FakturId", dto.FakturId, SqlDbType.VarChar);
            dp.AddParam("@FakturCode", dto.FakturCode, SqlDbType.VarChar);
            dp.AddParam("@FakturDate", dto.FakturDate, SqlDbType.DateTime);
            dp.AddParam("@AdminName", dto.AdminName, SqlDbType.VarChar);
            dp.AddParam("@GrandTotal", dto.GrandTotal, SqlDbType.Decimal);

            dp.AddParam("@DriverId", dto.DriverId, SqlDbType.VarChar);
            dp.AddParam("@DriverName", dto.DriverName, SqlDbType.VarChar);

            dp.AddParam("@DownloadTimestamp", dto.DownloadTimestamp, SqlDbType.DateTime);
            dp.AddParam("@OfficeCode", dto.OfficeCode, SqlDbType.VarChar);
            dp.AddParam("@PrintLogId", dto.PrintLogId, SqlDbType.VarChar);
            dp.AddParam("@Note", dto.Note, SqlDbType.VarChar);

            using (var conn = new SqlConnection(ConnStringHelper.Get(_opt)))
            {
                conn.Execute(sql, dp);
            }
        }

        public void Delete(IPackingOrderKey key)
        {
            const string sql = @"
                DELETE FROM BTRG_PackingOrder
                WHERE PackingOrderId = @PackingOrderId
                ";

            var dp = new DynamicParameters();
            dp.AddParam("@PackingOrderId", key.PackingOrderId, SqlDbType.VarChar);

            using (var conn = new SqlConnection(ConnStringHelper.Get(_opt)))
            {
                conn.Execute(sql, dp);
            }
        }

        public PackingOrderDto GetData(IPackingOrderKey key)
        {
            const string sql = @"
                SELECT
                    aa.PackingOrderId, aa.PackingOrderDate, 
                    aa.CustomerId, aa.CustomerCode, aa.CustomerName, aa.Alamat, aa.NoTelp,
                    aa.Latitude, aa.Longitude, aa.Accuracy,
                    aa.FakturId, aa.FakturCode, aa.FakturDate, aa.AdminName, aa.GrandTotal,
                    aa.DriverId, aa.DriverName, aa.DownloadTimestamp, aa.OfficeCode, aa.PrintLogId,
                    ISNULL(bb.PrintLogTimestamp, '3000-01-01') PrintTimestamp,
                    ISNULL(bb.DocType, '') DocType,
                    aa.Note
                FROM BTRG_PackingOrder aa
                    LEFT JOIN BTRG_PrintLog bb ON aa.PrintLogId = bb.PrintLogId
                WHERE PackingOrderId = @PackingOrderId
                ";

            var dp = new DynamicParameters();
            dp.AddParam("@PackingOrderId", key.PackingOrderId, SqlDbType.VarChar);

            using (var conn = new SqlConnection(ConnStringHelper.Get(_opt)))
            {
                return conn.ReadSingle<PackingOrderDto>(sql, dp);
            }
        }

        public IEnumerable<PackingOrderDto> ListData(Periode filter)
        {
            const string sql = @"
                SELECT
                    aa.PackingOrderId, aa.PackingOrderDate, 
                    aa.CustomerId, aa.CustomerCode, aa.CustomerName, aa.Alamat, aa.NoTelp,
                    aa.Latitude, aa.Longitude, aa.Accuracy,
                    aa.FakturId, aa.FakturCode, aa.FakturDate, aa.AdminName, aa.GrandTotal,
                    aa.DriverId, aa.DriverName, aa.DownloadTimestamp, aa.OfficeCode, aa.PrintLogId,
                    ISNULL(bb.PrintLogTimestamp, '3000-01-01') PrintTimestamp,
                    ISNULL(bb.DocType, '') DocType,
                    Note
                FROM BTRG_PackingOrder aa
                    LEFT JOIN BTRG_PrintLog bb ON aa.PrintLogId = bb.PrintLogId
S                WHERE PackingOrderDate BETWEEN @Tgl1 AND @Tgl2
                ";

            var dp = new DynamicParameters();
            dp.AddParam("@Tgl1", filter.Tgl1, SqlDbType.DateTime);
            dp.AddParam("@Tgl2", filter.Tgl2, SqlDbType.DateTime);

            using (var conn = new SqlConnection(ConnStringHelper.Get(_opt)))
            {
                return conn.Read<PackingOrderDto>(sql, dp);
            }
        }

        public IEnumerable<PackingOrderView> ListDataView(Periode periode)
        {
            const string sql = @"
                SELECT
                    aa.PackingOrderId, aa.FakturCode, aa.FakturDate, aa.GrandTotal,
                    aa.CustomerCode, aa.CustomerName, aa.DriverId, aa.DriverName,
                    aa.Alamat, aa.DownloadTimestamp, aa.PrintLogId, 
                    bb.PrintLogTimestamp AS JamCetak, ISNULL(bb.DocType,'') AS JenisCetak,
                    aa.Note
                FROM BTRG_PackingOrder aa
                    LEFT JOIN BTRG_PrintLog bb ON aa.PrintLogId = bb.PrintLogId
                WHERE FakturDate BETWEEN @Tgl1 AND @Tgl2
                ";

            var dp = new DynamicParameters();
            dp.AddParam("@Tgl1", periode.Tgl1, SqlDbType.DateTime);
            dp.AddParam("@Tgl2", periode.Tgl2, SqlDbType.DateTime);

            using (var conn = new SqlConnection(ConnStringHelper.Get(_opt)))
            {
                return conn.Read<PackingOrderView>(sql, dp);
            }
        }

        public IEnumerable<PackingOrderView> ListByDownloadTimestamp(DateTime downloadTimestamp)
        {
            const string sql = @"
                SELECT
                    PackingOrderId, FakturCode, FakturDate, GrandTotal,
                    CustomerCode, CustomerName, DriverId, DriverName,
                    Alamat, DownloadTimestamp, PrintLogId, Note
                FROM BTRG_PackingOrder
                WHERE DownloadTimestamp BETWEEN @Tgl1 AND @Tgl2
                ";

            var dp = new DynamicParameters();
            var startDate = downloadTimestamp.Date;
            var endDate = downloadTimestamp.Date.AddDays(1).AddSeconds(-1);
            dp.AddParam("@Tgl1", startDate, SqlDbType.DateTime);
            dp.AddParam("@Tgl2", endDate, SqlDbType.DateTime);

            using (var conn = new SqlConnection(ConnStringHelper.Get(_opt)))
            {
                return conn.Read<PackingOrderView>(sql, dp);
            }
        }
        public PackingOrderView GetLastDownloadByTimestamp()
        {
            const string sql = @"
                SELECT TOP 1
                    PackingOrderId, FakturCode, FakturDate, GrandTotal,
                    CustomerCode, CustomerName, DriverId, DriverName,
                    Alamat, DownloadTimestamp, PrintLogId, Note
                FROM BTRG_PackingOrder
                ORDER BY DownloadTimestamp DESC
                ";

            using (var conn = new SqlConnection(ConnStringHelper.Get(_opt)))
            {
                var result = conn.Read<PackingOrderView>(sql);
                return result.FirstOrDefault();
            }
        }
    }
}