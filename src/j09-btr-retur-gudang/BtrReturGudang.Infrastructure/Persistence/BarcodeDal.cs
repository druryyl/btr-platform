using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using BtrReturGudang.AppTier.Repositories;
using BtrReturGudang.Domain.Entities;
using BtrReturGudang.Domain.Enums;
using BtrReturGudang.Domain.ValueObjects;
using BtrReturGudang.Infrastructure.Helpers;
using Dapper;

namespace BtrReturGudang.Infrastructure.Persistence
{
    /// <summary>
    /// Dapper persistence implementation for IBarcodeRepo against local SQL Server (btrgd).
    /// Manages BTRG_Barcode and BTRG_BarcodeRegistration tables.
    /// Traceable to: ARCHITECTURE §5.1, §8, P2-S04.
    /// </summary>
    public class BarcodeDal : IBarcodeRepo
    {
        private readonly string _connectionString;

        public BarcodeDal(string connectionString = null)
        {
            _connectionString = connectionString;
        }

        private SqlConnection CreateConnection() => ConnStringHelper.CreateConnection(_connectionString);

        public Barcode LookupByBarcode(string barcode)
        {
            var normalized = BarcodeNormalization.Normalize(barcode);
            if (string.IsNullOrWhiteSpace(normalized))
                return null;

            using (var conn = CreateConnection())
            {
                conn.Open();
                const string sql = @"
SELECT Barcode, BrgId, BrgName, Satuan, Pengali, IsActive
FROM BTRG_Barcode
WHERE Barcode = @Barcode AND IsActive = 1;";

                var row = conn.QueryFirstOrDefault<BarcodeRow>(sql, new { Barcode = normalized });
                return row != null ? MapToBarcode(row) : null;
            }
        }

        public async Task<Barcode> LookupByBarcodeAsync(string barcode)
        {
            var normalized = BarcodeNormalization.Normalize(barcode);
            if (string.IsNullOrWhiteSpace(normalized))
                return null;

            using (var conn = CreateConnection())
            {
                await conn.OpenAsync().ConfigureAwait(false);
                const string sql = @"
SELECT Barcode, BrgId, BrgName, Satuan, Pengali, IsActive
FROM BTRG_Barcode
WHERE Barcode = @Barcode AND IsActive = 1;";

                var row = await conn.QueryFirstOrDefaultAsync<BarcodeRow>(sql, new { Barcode = normalized }).ConfigureAwait(false);
                return row != null ? MapToBarcode(row) : null;
            }
        }

        public void InsertBarcode(Barcode barcode)
        {
            if (barcode == null)
                throw new ArgumentNullException(nameof(barcode));

            using (var conn = CreateConnection())
            {
                conn.Open();
                const string sql = @"
IF EXISTS (SELECT 1 FROM BTRG_Barcode WHERE Barcode = @Barcode)
BEGIN
    UPDATE BTRG_Barcode
    SET BrgId = @BrgId,
        BrgName = @BrgName,
        Satuan = @Satuan,
        Pengali = @Pengali,
        IsActive = @IsActive
    WHERE Barcode = @Barcode;
END
ELSE
BEGIN
    INSERT INTO BTRG_Barcode (Barcode, BrgId, BrgName, Satuan, Pengali, IsActive)
    VALUES (@Barcode, @BrgId, @BrgName, @Satuan, @Pengali, @IsActive);
END;";

                conn.Execute(sql, new
                {
                    Barcode = barcode.BarcodeValue,
                    barcode.BrgId,
                    barcode.BrgName,
                    barcode.Satuan,
                    barcode.Pengali,
                    barcode.IsActive
                });
            }
        }

        public async Task InsertBarcodeAsync(Barcode barcode)
        {
            if (barcode == null)
                throw new ArgumentNullException(nameof(barcode));

            using (var conn = CreateConnection())
            {
                await conn.OpenAsync().ConfigureAwait(false);
                const string sql = @"
IF EXISTS (SELECT 1 FROM BTRG_Barcode WHERE Barcode = @Barcode)
BEGIN
    UPDATE BTRG_Barcode
    SET BrgId = @BrgId,
        BrgName = @BrgName,
        Satuan = @Satuan,
        Pengali = @Pengali,
        IsActive = @IsActive
    WHERE Barcode = @Barcode;
END
ELSE
BEGIN
    INSERT INTO BTRG_Barcode (Barcode, BrgId, BrgName, Satuan, Pengali, IsActive)
    VALUES (@Barcode, @BrgId, @BrgName, @Satuan, @Pengali, @IsActive);
END;";

                await conn.ExecuteAsync(sql, new
                {
                    Barcode = barcode.BarcodeValue,
                    barcode.BrgId,
                    barcode.BrgName,
                    barcode.Satuan,
                    barcode.Pengali,
                    barcode.IsActive
                }).ConfigureAwait(false);
            }
        }

        public void BulkUpsertBarcodes(IEnumerable<Barcode> barcodes)
        {
            if (barcodes == null)
                return;

            var list = barcodes.ToList();
            if (!list.Any())
                return;

            using (var conn = CreateConnection())
            {
                conn.Open();
                using (var tran = conn.BeginTransaction())
                {
                    try
                    {
                        const string sql = @"
IF EXISTS (SELECT 1 FROM BTRG_Barcode WHERE Barcode = @Barcode)
BEGIN
    UPDATE BTRG_Barcode
    SET BrgId = @BrgId,
        BrgName = @BrgName,
        Satuan = @Satuan,
        Pengali = @Pengali,
        IsActive = @IsActive
    WHERE Barcode = @Barcode;
END
ELSE
BEGIN
    INSERT INTO BTRG_Barcode (Barcode, BrgId, BrgName, Satuan, Pengali, IsActive)
    VALUES (@Barcode, @BrgId, @BrgName, @Satuan, @Pengali, @IsActive);
END;";

                        var paramList = list.Select(b => new
                        {
                            Barcode = b.BarcodeValue,
                            b.BrgId,
                            b.BrgName,
                            b.Satuan,
                            b.Pengali,
                            b.IsActive
                        });

                        conn.Execute(sql, paramList, tran);
                        tran.Commit();
                    }
                    catch
                    {
                        tran.Rollback();
                        throw;
                    }
                }
            }
        }

        public async Task BulkUpsertBarcodesAsync(IEnumerable<Barcode> barcodes)
        {
            if (barcodes == null)
                return;

            var list = barcodes.ToList();
            if (!list.Any())
                return;

            using (var conn = CreateConnection())
            {
                await conn.OpenAsync().ConfigureAwait(false);
                using (var tran = conn.BeginTransaction())
                {
                    try
                    {
                        const string sql = @"
IF EXISTS (SELECT 1 FROM BTRG_Barcode WHERE Barcode = @Barcode)
BEGIN
    UPDATE BTRG_Barcode
    SET BrgId = @BrgId,
        BrgName = @BrgName,
        Satuan = @Satuan,
        Pengali = @Pengali,
        IsActive = @IsActive
    WHERE Barcode = @Barcode;
END
ELSE
BEGIN
    INSERT INTO BTRG_Barcode (Barcode, BrgId, BrgName, Satuan, Pengali, IsActive)
    VALUES (@Barcode, @BrgId, @BrgName, @Satuan, @Pengali, @IsActive);
END;";

                        var paramList = list.Select(b => new
                        {
                            Barcode = b.BarcodeValue,
                            b.BrgId,
                            b.BrgName,
                            b.Satuan,
                            b.Pengali,
                            b.IsActive
                        });

                        await conn.ExecuteAsync(sql, paramList, tran).ConfigureAwait(false);
                        tran.Commit();
                    }
                    catch
                    {
                        tran.Rollback();
                        throw;
                    }
                }
            }
        }

        public IEnumerable<Barcode> GetAllBarcodes()
        {
            using (var conn = CreateConnection())
            {
                conn.Open();
                const string sql = @"
SELECT Barcode, BrgId, BrgName, Satuan, Pengali, IsActive
FROM BTRG_Barcode
ORDER BY BrgName, Barcode;";

                var rows = conn.Query<BarcodeRow>(sql);
                return rows.Select(MapToBarcode).ToList();
            }
        }

        public async Task<IEnumerable<Barcode>> GetAllBarcodesAsync()
        {
            using (var conn = CreateConnection())
            {
                await conn.OpenAsync().ConfigureAwait(false);
                const string sql = @"
SELECT Barcode, BrgId, BrgName, Satuan, Pengali, IsActive
FROM BTRG_Barcode
ORDER BY BrgName, Barcode;";

                var rows = await conn.QueryAsync<BarcodeRow>(sql).ConfigureAwait(false);
                return rows.Select(MapToBarcode).ToList();
            }
        }

        public IEnumerable<Barcode> SearchBarcodes(string keyword)
        {
            var searchKey = string.IsNullOrWhiteSpace(keyword) ? "%" : $"%{keyword.Trim()}%";
            using (var conn = CreateConnection())
            {
                conn.Open();
                const string sql = @"
SELECT Barcode, BrgId, BrgName, Satuan, Pengali, IsActive
FROM BTRG_Barcode
WHERE Barcode LIKE @Keyword OR BrgName LIKE @Keyword OR BrgId LIKE @Keyword
ORDER BY BrgName, Barcode;";

                var rows = conn.Query<BarcodeRow>(sql, new { Keyword = searchKey });
                return rows.Select(MapToBarcode).ToList();
            }
        }

        public async Task<IEnumerable<Barcode>> SearchBarcodesAsync(string keyword)
        {
            var searchKey = string.IsNullOrWhiteSpace(keyword) ? "%" : $"%{keyword.Trim()}%";
            using (var conn = CreateConnection())
            {
                await conn.OpenAsync().ConfigureAwait(false);
                const string sql = @"
SELECT Barcode, BrgId, BrgName, Satuan, Pengali, IsActive
FROM BTRG_Barcode
WHERE Barcode LIKE @Keyword OR BrgName LIKE @Keyword OR BrgId LIKE @Keyword
ORDER BY BrgName, Barcode;";

                var rows = await conn.QueryAsync<BarcodeRow>(sql, new { Keyword = searchKey }).ConfigureAwait(false);
                return rows.Select(MapToBarcode).ToList();
            }
        }

        public void QueueRegistration(BarcodeRegistration registration)
        {
            if (registration == null)
                throw new ArgumentNullException(nameof(registration));

            using (var conn = CreateConnection())
            {
                conn.Open();
                const string sql = @"
INSERT INTO BTRG_BarcodeRegistration (
    RequestId, Barcode, BrgId, Satuan, WarehouseCode, OperatorName, StatusSync, CreatedDate
) VALUES (
    @RequestId, @Barcode, @BrgId, @Satuan, @WarehouseCode, @OperatorName, @StatusSync, @CreatedDate
);";

                conn.Execute(sql, new
                {
                    registration.RequestId,
                    registration.Barcode,
                    registration.BrgId,
                    registration.Satuan,
                    registration.WarehouseCode,
                    registration.OperatorName,
                    StatusSync = registration.StatusSync.ToString(),
                    registration.CreatedDate
                });
            }
        }

        public async Task QueueRegistrationAsync(BarcodeRegistration registration)
        {
            if (registration == null)
                throw new ArgumentNullException(nameof(registration));

            using (var conn = CreateConnection())
            {
                await conn.OpenAsync().ConfigureAwait(false);
                const string sql = @"
INSERT INTO BTRG_BarcodeRegistration (
    RequestId, Barcode, BrgId, Satuan, WarehouseCode, OperatorName, StatusSync, CreatedDate
) VALUES (
    @RequestId, @Barcode, @BrgId, @Satuan, @WarehouseCode, @OperatorName, @StatusSync, @CreatedDate
);";

                await conn.ExecuteAsync(sql, new
                {
                    registration.RequestId,
                    registration.Barcode,
                    registration.BrgId,
                    registration.Satuan,
                    registration.WarehouseCode,
                    registration.OperatorName,
                    StatusSync = registration.StatusSync.ToString(),
                    registration.CreatedDate
                }).ConfigureAwait(false);
            }
        }

        public IEnumerable<BarcodeRegistration> GetPendingRegistrations()
        {
            using (var conn = CreateConnection())
            {
                conn.Open();
                const string sql = @"
SELECT RequestId, Barcode, BrgId, Satuan, WarehouseCode, OperatorName, StatusSync, CreatedDate
FROM BTRG_BarcodeRegistration
WHERE StatusSync = 'Pending'
ORDER BY CreatedDate ASC;";

                var rows = conn.Query<BarcodeRegistrationRow>(sql);
                return rows.Select(MapToRegistration).ToList();
            }
        }

        public async Task<IEnumerable<BarcodeRegistration>> GetPendingRegistrationsAsync()
        {
            using (var conn = CreateConnection())
            {
                await conn.OpenAsync().ConfigureAwait(false);
                const string sql = @"
SELECT RequestId, Barcode, BrgId, Satuan, WarehouseCode, OperatorName, StatusSync, CreatedDate
FROM BTRG_BarcodeRegistration
WHERE StatusSync = 'Pending'
ORDER BY CreatedDate ASC;";

                var rows = await conn.QueryAsync<BarcodeRegistrationRow>(sql).ConfigureAwait(false);
                return rows.Select(MapToRegistration).ToList();
            }
        }

        public void MarkRegistrationUploaded(string requestId)
        {
            if (string.IsNullOrWhiteSpace(requestId))
                throw new ArgumentException("RequestId cannot be null or empty.", nameof(requestId));

            using (var conn = CreateConnection())
            {
                conn.Open();
                const string sql = @"
UPDATE BTRG_BarcodeRegistration
SET StatusSync = 'Uploaded'
WHERE RequestId = @RequestId;";

                conn.Execute(sql, new { RequestId = requestId });
            }
        }

        public async Task MarkRegistrationUploadedAsync(string requestId)
        {
            if (string.IsNullOrWhiteSpace(requestId))
                throw new ArgumentException("RequestId cannot be null or empty.", nameof(requestId));

            using (var conn = CreateConnection())
            {
                await conn.OpenAsync().ConfigureAwait(false);
                const string sql = @"
UPDATE BTRG_BarcodeRegistration
SET StatusSync = 'Uploaded'
WHERE RequestId = @RequestId;";

                await conn.ExecuteAsync(sql, new { RequestId = requestId }).ConfigureAwait(false);
            }
        }

        private static Barcode MapToBarcode(BarcodeRow row)
        {
            return new Barcode(row.Barcode, row.BrgId, row.BrgName, row.Satuan, row.Pengali, row.IsActive);
        }

        private static BarcodeRegistration MapToRegistration(BarcodeRegistrationRow row)
        {
            var status = Enum.TryParse<BarcodeRegistrationStatus>(row.StatusSync, true, out var s) ? s : BarcodeRegistrationStatus.Pending;
            return new BarcodeRegistration(
                row.RequestId,
                row.Barcode,
                row.BrgId,
                row.Satuan,
                row.WarehouseCode,
                row.OperatorName,
                status,
                row.CreatedDate);
        }

        private class BarcodeRow
        {
            public string Barcode { get; set; }
            public string BrgId { get; set; }
            public string BrgName { get; set; }
            public string Satuan { get; set; }
            public decimal Pengali { get; set; }
            public bool IsActive { get; set; }
        }

        private class BarcodeRegistrationRow
        {
            public string RequestId { get; set; }
            public string Barcode { get; set; }
            public string BrgId { get; set; }
            public string Satuan { get; set; }
            public string WarehouseCode { get; set; }
            public string OperatorName { get; set; }
            public string StatusSync { get; set; }
            public DateTime CreatedDate { get; set; }
        }
    }
}
