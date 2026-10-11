using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using BtrReturGudang.AppTier.Repositories;
using BtrReturGudang.Domain.Entities;
using BtrReturGudang.Domain.Enums;
using BtrReturGudang.Domain.Exceptions;
using BtrReturGudang.Infrastructure.Helpers;
using Dapper;

namespace BtrReturGudang.Infrastructure.Persistence
{
    /// <summary>
    /// Dapper persistence implementation for IReturnOrderRepo against local SQL Server (btrgd).
    /// Enforces transaction scope on header + items persistence and post-sync immutability.
    /// Traceable to: ARCHITECTURE §5.1, §8, TD-010, P2-S04.
    /// </summary>
    public class ReturnOrderDal : IReturnOrderRepo
    {
        private readonly string _connectionString;

        public ReturnOrderDal(string connectionString = null)
        {
            _connectionString = connectionString;
        }

        private SqlConnection CreateConnection() => ConnStringHelper.CreateConnection(_connectionString);

        public ReturnOrder GetById(string returnOrderId)
        {
            if (string.IsNullOrWhiteSpace(returnOrderId))
                return null;

            using (var conn = CreateConnection())
            {
                conn.Open();
                const string sql = @"
SELECT ReturnOrderId, ReturnOrderDate, CustId, CustName, DriverId, DriverName,
       SalesPersonId, SalesPersonName, WarehouseCode, OperatorName, Keterangan,
       StatusSync, CreatedDate, ModifiedDate, SyncedDate
FROM BTRG_ReturnOrder
WHERE ReturnOrderId = @ReturnOrderId;

SELECT ReturnOrderId, NoUrut, BrgId, BrgName, Barcode, JenisRetur, Qty, SatId, Keterangan
FROM BTRG_ReturnOrderItem
WHERE ReturnOrderId = @ReturnOrderId
ORDER BY NoUrut ASC;";

                using (var multi = conn.QueryMultiple(sql, new { ReturnOrderId = returnOrderId }))
                {
                    var headerRow = multi.ReadFirstOrDefault<ReturnOrderHeaderRow>();
                    if (headerRow == null)
                        return null;

                    var itemRows = multi.Read<ReturnOrderItemRow>().ToList();
                    return MapToDomain(headerRow, itemRows);
                }
            }
        }

        public async Task<ReturnOrder> GetByIdAsync(string returnOrderId)
        {
            if (string.IsNullOrWhiteSpace(returnOrderId))
                return null;

            using (var conn = CreateConnection())
            {
                await conn.OpenAsync().ConfigureAwait(false);
                const string sql = @"
SELECT ReturnOrderId, ReturnOrderDate, CustId, CustName, DriverId, DriverName,
       SalesPersonId, SalesPersonName, WarehouseCode, OperatorName, Keterangan,
       StatusSync, CreatedDate, ModifiedDate, SyncedDate
FROM BTRG_ReturnOrder
WHERE ReturnOrderId = @ReturnOrderId;

SELECT ReturnOrderId, NoUrut, BrgId, BrgName, Barcode, JenisRetur, Qty, SatId, Keterangan
FROM BTRG_ReturnOrderItem
WHERE ReturnOrderId = @ReturnOrderId
ORDER BY NoUrut ASC;";

                using (var multi = await conn.QueryMultipleAsync(sql, new { ReturnOrderId = returnOrderId }).ConfigureAwait(false))
                {
                    var headerRow = await multi.ReadFirstOrDefaultAsync<ReturnOrderHeaderRow>().ConfigureAwait(false);
                    if (headerRow == null)
                        return null;

                    var itemRows = (await multi.ReadAsync<ReturnOrderItemRow>().ConfigureAwait(false)).ToList();
                    return MapToDomain(headerRow, itemRows);
                }
            }
        }

        public IEnumerable<ReturnOrder> GetFilteredList(DateTime startDate, DateTime endDate, StatusSync? statusFilter = null)
        {
            using (var conn = CreateConnection())
            {
                conn.Open();
                const string headerSql = @"
SELECT ReturnOrderId, ReturnOrderDate, CustId, CustName, DriverId, DriverName,
       SalesPersonId, SalesPersonName, WarehouseCode, OperatorName, Keterangan,
       StatusSync, CreatedDate, ModifiedDate, SyncedDate
FROM BTRG_ReturnOrder
WHERE ReturnOrderDate >= @StartDate AND ReturnOrderDate <= @EndDate
  AND (@StatusSync IS NULL OR StatusSync = @StatusSync)
ORDER BY ReturnOrderDate DESC, CreatedDate DESC;";

                var statusParam = statusFilter?.ToString();
                var headers = conn.Query<ReturnOrderHeaderRow>(headerSql, new
                {
                    StartDate = startDate.Date,
                    EndDate = endDate.Date,
                    StatusSync = statusParam
                }).ToList();

                if (!headers.Any())
                    return Enumerable.Empty<ReturnOrder>();

                var orderIds = headers.Select(h => h.ReturnOrderId).ToList();
                const string itemSql = @"
SELECT ReturnOrderId, NoUrut, BrgId, BrgName, Barcode, JenisRetur, Qty, SatId, Keterangan
FROM BTRG_ReturnOrderItem
WHERE ReturnOrderId IN @OrderIds
ORDER BY ReturnOrderId, NoUrut ASC;";

                var items = conn.Query<ReturnOrderItemRow>(itemSql, new { OrderIds = orderIds })
                    .GroupBy(i => i.ReturnOrderId)
                    .ToDictionary(g => g.Key, g => g.ToList());

                return headers.Select(h =>
                {
                    items.TryGetValue(h.ReturnOrderId, out var orderItems);
                    return MapToDomain(h, orderItems ?? new List<ReturnOrderItemRow>());
                }).ToList();
            }
        }

        public async Task<IEnumerable<ReturnOrder>> GetFilteredListAsync(DateTime startDate, DateTime endDate, StatusSync? statusFilter = null)
        {
            using (var conn = CreateConnection())
            {
                await conn.OpenAsync().ConfigureAwait(false);
                const string headerSql = @"
SELECT ReturnOrderId, ReturnOrderDate, CustId, CustName, DriverId, DriverName,
       SalesPersonId, SalesPersonName, WarehouseCode, OperatorName, Keterangan,
       StatusSync, CreatedDate, ModifiedDate, SyncedDate
FROM BTRG_ReturnOrder
WHERE ReturnOrderDate >= @StartDate AND ReturnOrderDate <= @EndDate
  AND (@StatusSync IS NULL OR StatusSync = @StatusSync)
ORDER BY ReturnOrderDate DESC, CreatedDate DESC;";

                var statusParam = statusFilter?.ToString();
                var headers = (await conn.QueryAsync<ReturnOrderHeaderRow>(headerSql, new
                {
                    StartDate = startDate.Date,
                    EndDate = endDate.Date,
                    StatusSync = statusParam
                }).ConfigureAwait(false)).ToList();

                if (!headers.Any())
                    return Enumerable.Empty<ReturnOrder>();

                var orderIds = headers.Select(h => h.ReturnOrderId).ToList();
                const string itemSql = @"
SELECT ReturnOrderId, NoUrut, BrgId, BrgName, Barcode, JenisRetur, Qty, SatId, Keterangan
FROM BTRG_ReturnOrderItem
WHERE ReturnOrderId IN @OrderIds
ORDER BY ReturnOrderId, NoUrut ASC;";

                var itemResult = await conn.QueryAsync<ReturnOrderItemRow>(itemSql, new { OrderIds = orderIds }).ConfigureAwait(false);
                var items = itemResult.GroupBy(i => i.ReturnOrderId)
                    .ToDictionary(g => g.Key, g => g.ToList());

                return headers.Select(h =>
                {
                    items.TryGetValue(h.ReturnOrderId, out var orderItems);
                    return MapToDomain(h, orderItems ?? new List<ReturnOrderItemRow>());
                }).ToList();
            }
        }

        public IEnumerable<ReturnOrder> GetPendingUploadList()
        {
            using (var conn = CreateConnection())
            {
                conn.Open();
                const string headerSql = @"
SELECT ReturnOrderId, ReturnOrderDate, CustId, CustName, DriverId, DriverName,
       SalesPersonId, SalesPersonName, WarehouseCode, OperatorName, Keterangan,
       StatusSync, CreatedDate, ModifiedDate, SyncedDate
FROM BTRG_ReturnOrder
WHERE StatusSync = 'Draft'
ORDER BY CreatedDate ASC;";

                var headers = conn.Query<ReturnOrderHeaderRow>(headerSql).ToList();
                if (!headers.Any())
                    return Enumerable.Empty<ReturnOrder>();

                var orderIds = headers.Select(h => h.ReturnOrderId).ToList();
                const string itemSql = @"
SELECT ReturnOrderId, NoUrut, BrgId, BrgName, Barcode, JenisRetur, Qty, SatId, Keterangan
FROM BTRG_ReturnOrderItem
WHERE ReturnOrderId IN @OrderIds
ORDER BY ReturnOrderId, NoUrut ASC;";

                var items = conn.Query<ReturnOrderItemRow>(itemSql, new { OrderIds = orderIds })
                    .GroupBy(i => i.ReturnOrderId)
                    .ToDictionary(g => g.Key, g => g.ToList());

                return headers.Select(h =>
                {
                    items.TryGetValue(h.ReturnOrderId, out var orderItems);
                    return MapToDomain(h, orderItems ?? new List<ReturnOrderItemRow>());
                }).ToList();
            }
        }

        public async Task<IEnumerable<ReturnOrder>> GetPendingUploadListAsync()
        {
            using (var conn = CreateConnection())
            {
                await conn.OpenAsync().ConfigureAwait(false);
                const string headerSql = @"
SELECT ReturnOrderId, ReturnOrderDate, CustId, CustName, DriverId, DriverName,
       SalesPersonId, SalesPersonName, WarehouseCode, OperatorName, Keterangan,
       StatusSync, CreatedDate, ModifiedDate, SyncedDate
FROM BTRG_ReturnOrder
WHERE StatusSync = 'Draft'
ORDER BY CreatedDate ASC;";

                var headers = (await conn.QueryAsync<ReturnOrderHeaderRow>(headerSql).ConfigureAwait(false)).ToList();
                if (!headers.Any())
                    return Enumerable.Empty<ReturnOrder>();

                var orderIds = headers.Select(h => h.ReturnOrderId).ToList();
                const string itemSql = @"
SELECT ReturnOrderId, NoUrut, BrgId, BrgName, Barcode, JenisRetur, Qty, SatId, Keterangan
FROM BTRG_ReturnOrderItem
WHERE ReturnOrderId IN @OrderIds
ORDER BY ReturnOrderId, NoUrut ASC;";

                var itemResult = await conn.QueryAsync<ReturnOrderItemRow>(itemSql, new { OrderIds = orderIds }).ConfigureAwait(false);
                var items = itemResult.GroupBy(i => i.ReturnOrderId)
                    .ToDictionary(g => g.Key, g => g.ToList());

                return headers.Select(h =>
                {
                    items.TryGetValue(h.ReturnOrderId, out var orderItems);
                    return MapToDomain(h, orderItems ?? new List<ReturnOrderItemRow>());
                }).ToList();
            }
        }

        public void Save(ReturnOrder returnOrder)
        {
            if (returnOrder == null)
                throw new ArgumentNullException(nameof(returnOrder));

            returnOrder.Validate();

            using (var conn = CreateConnection())
            {
                conn.Open();

                // Check existing status to guard immutability
                const string checkSql = "SELECT StatusSync FROM BTRG_ReturnOrder WHERE ReturnOrderId = @ReturnOrderId;";
                var existingStatus = conn.QueryFirstOrDefault<string>(checkSql, new { returnOrder.ReturnOrderId });
                if (existingStatus != null && string.Equals(existingStatus, StatusSync.Synced.ToString(), StringComparison.OrdinalIgnoreCase))
                {
                    throw new DocumentLockedException(returnOrder.ReturnOrderId, "Save");
                }

                using (var tran = conn.BeginTransaction())
                {
                    try
                    {
                        const string upsertHeaderSql = @"
IF EXISTS (SELECT 1 FROM BTRG_ReturnOrder WHERE ReturnOrderId = @ReturnOrderId)
BEGIN
    UPDATE BTRG_ReturnOrder SET
        ReturnOrderDate = @ReturnOrderDate,
        CustId = @CustId,
        CustName = @CustName,
        DriverId = @DriverId,
        DriverName = @DriverName,
        SalesPersonId = @SalesPersonId,
        SalesPersonName = @SalesPersonName,
        WarehouseCode = @WarehouseCode,
        OperatorName = @OperatorName,
        Keterangan = @Keterangan,
        StatusSync = @StatusSync,
        ModifiedDate = @ModifiedDate,
        SyncedDate = @SyncedDate
    WHERE ReturnOrderId = @ReturnOrderId;
END
ELSE
BEGIN
    INSERT INTO BTRG_ReturnOrder (
        ReturnOrderId, ReturnOrderDate, CustId, CustName,
        DriverId, DriverName, SalesPersonId, SalesPersonName,
        WarehouseCode, OperatorName, Keterangan, StatusSync,
        CreatedDate, ModifiedDate, SyncedDate
    ) VALUES (
        @ReturnOrderId, @ReturnOrderDate, @CustId, @CustName,
        @DriverId, @DriverName, @SalesPersonId, @SalesPersonName,
        @WarehouseCode, @OperatorName, @Keterangan, @StatusSync,
        @CreatedDate, @ModifiedDate, @SyncedDate
    );
END;";

                        var headerParams = new
                        {
                            returnOrder.ReturnOrderId,
                            ReturnOrderDate = returnOrder.ReturnOrderDate.Date,
                            returnOrder.CustId,
                            returnOrder.CustName,
                            returnOrder.DriverId,
                            returnOrder.DriverName,
                            returnOrder.SalesPersonId,
                            returnOrder.SalesPersonName,
                            returnOrder.WarehouseCode,
                            returnOrder.OperatorName,
                            returnOrder.Keterangan,
                            StatusSync = returnOrder.StatusSync.ToString(),
                            returnOrder.CreatedDate,
                            returnOrder.ModifiedDate,
                            returnOrder.SyncedDate
                        };

                        conn.Execute(upsertHeaderSql, headerParams, tran);

                        // Detail item replacement
                        const string deleteItemsSql = "DELETE FROM BTRG_ReturnOrderItem WHERE ReturnOrderId = @ReturnOrderId;";
                        conn.Execute(deleteItemsSql, new { returnOrder.ReturnOrderId }, tran);

                        if (returnOrder.Items.Any())
                        {
                            const string insertItemSql = @"
INSERT INTO BTRG_ReturnOrderItem (
    ReturnOrderId, NoUrut, BrgId, BrgName, Barcode, JenisRetur, Qty, SatId, Keterangan
) VALUES (
    @ReturnOrderId, @NoUrut, @BrgId, @BrgName, @Barcode, @JenisRetur, @Qty, @SatId, @Keterangan
);";

                            var itemParams = returnOrder.Items.Select(i => new
                            {
                                ReturnOrderId = returnOrder.ReturnOrderId,
                                i.NoUrut,
                                i.BrgId,
                                i.BrgName,
                                i.Barcode,
                                JenisRetur = i.JenisRetur.ToString(),
                                i.Qty,
                                i.SatId,
                                i.Keterangan
                            });

                            conn.Execute(insertItemSql, itemParams, tran);
                        }

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

        public async Task SaveAsync(ReturnOrder returnOrder)
        {
            if (returnOrder == null)
                throw new ArgumentNullException(nameof(returnOrder));

            returnOrder.Validate();

            using (var conn = CreateConnection())
            {
                await conn.OpenAsync().ConfigureAwait(false);

                // Check existing status to guard immutability
                const string checkSql = "SELECT StatusSync FROM BTRG_ReturnOrder WHERE ReturnOrderId = @ReturnOrderId;";
                var existingStatus = await conn.QueryFirstOrDefaultAsync<string>(checkSql, new { returnOrder.ReturnOrderId }).ConfigureAwait(false);
                if (existingStatus != null && string.Equals(existingStatus, StatusSync.Synced.ToString(), StringComparison.OrdinalIgnoreCase))
                {
                    throw new DocumentLockedException(returnOrder.ReturnOrderId, "Save");
                }

                using (var tran = conn.BeginTransaction())
                {
                    try
                    {
                        const string upsertHeaderSql = @"
IF EXISTS (SELECT 1 FROM BTRG_ReturnOrder WHERE ReturnOrderId = @ReturnOrderId)
BEGIN
    UPDATE BTRG_ReturnOrder SET
        ReturnOrderDate = @ReturnOrderDate,
        CustId = @CustId,
        CustName = @CustName,
        DriverId = @DriverId,
        DriverName = @DriverName,
        SalesPersonId = @SalesPersonId,
        SalesPersonName = @SalesPersonName,
        WarehouseCode = @WarehouseCode,
        OperatorName = @OperatorName,
        Keterangan = @Keterangan,
        StatusSync = @StatusSync,
        ModifiedDate = @ModifiedDate,
        SyncedDate = @SyncedDate
    WHERE ReturnOrderId = @ReturnOrderId;
END
ELSE
BEGIN
    INSERT INTO BTRG_ReturnOrder (
        ReturnOrderId, ReturnOrderDate, CustId, CustName,
        DriverId, DriverName, SalesPersonId, SalesPersonName,
        WarehouseCode, OperatorName, Keterangan, StatusSync,
        CreatedDate, ModifiedDate, SyncedDate
    ) VALUES (
        @ReturnOrderId, @ReturnOrderDate, @CustId, @CustName,
        @DriverId, @DriverName, @SalesPersonId, @SalesPersonName,
        @WarehouseCode, @OperatorName, @Keterangan, @StatusSync,
        @CreatedDate, @ModifiedDate, @SyncedDate
    );
END;";

                        var headerParams = new
                        {
                            returnOrder.ReturnOrderId,
                            ReturnOrderDate = returnOrder.ReturnOrderDate.Date,
                            returnOrder.CustId,
                            returnOrder.CustName,
                            returnOrder.DriverId,
                            returnOrder.DriverName,
                            returnOrder.SalesPersonId,
                            returnOrder.SalesPersonName,
                            returnOrder.WarehouseCode,
                            returnOrder.OperatorName,
                            returnOrder.Keterangan,
                            StatusSync = returnOrder.StatusSync.ToString(),
                            returnOrder.CreatedDate,
                            returnOrder.ModifiedDate,
                            returnOrder.SyncedDate
                        };

                        await conn.ExecuteAsync(upsertHeaderSql, headerParams, tran).ConfigureAwait(false);

                        // Detail item replacement
                        const string deleteItemsSql = "DELETE FROM BTRG_ReturnOrderItem WHERE ReturnOrderId = @ReturnOrderId;";
                        await conn.ExecuteAsync(deleteItemsSql, new { returnOrder.ReturnOrderId }, tran).ConfigureAwait(false);

                        if (returnOrder.Items.Any())
                        {
                            const string insertItemSql = @"
INSERT INTO BTRG_ReturnOrderItem (
    ReturnOrderId, NoUrut, BrgId, BrgName, Barcode, JenisRetur, Qty, SatId, Keterangan
) VALUES (
    @ReturnOrderId, @NoUrut, @BrgId, @BrgName, @Barcode, @JenisRetur, @Qty, @SatId, @Keterangan
);";

                            var itemParams = returnOrder.Items.Select(i => new
                            {
                                ReturnOrderId = returnOrder.ReturnOrderId,
                                i.NoUrut,
                                i.BrgId,
                                i.BrgName,
                                i.Barcode,
                                JenisRetur = i.JenisRetur.ToString(),
                                i.Qty,
                                i.SatId,
                                i.Keterangan
                            });

                            await conn.ExecuteAsync(insertItemSql, itemParams, tran).ConfigureAwait(false);
                        }

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

        public void MarkSynced(string returnOrderId, DateTime syncedDate)
        {
            if (string.IsNullOrWhiteSpace(returnOrderId))
                throw new ArgumentException("ReturnOrderId cannot be null or empty.", nameof(returnOrderId));

            using (var conn = CreateConnection())
            {
                conn.Open();
                const string sql = @"
UPDATE BTRG_ReturnOrder
SET StatusSync = 'Synced',
    SyncedDate = @SyncedDate,
    ModifiedDate = GETDATE()
WHERE ReturnOrderId = @ReturnOrderId;";

                conn.Execute(sql, new { ReturnOrderId = returnOrderId, SyncedDate = syncedDate });
            }
        }

        public async Task MarkSyncedAsync(string returnOrderId, DateTime syncedDate)
        {
            if (string.IsNullOrWhiteSpace(returnOrderId))
                throw new ArgumentException("ReturnOrderId cannot be null or empty.", nameof(returnOrderId));

            using (var conn = CreateConnection())
            {
                await conn.OpenAsync().ConfigureAwait(false);
                const string sql = @"
UPDATE BTRG_ReturnOrder
SET StatusSync = 'Synced',
    SyncedDate = @SyncedDate,
    ModifiedDate = GETDATE()
WHERE ReturnOrderId = @ReturnOrderId;";

                await conn.ExecuteAsync(sql, new { ReturnOrderId = returnOrderId, SyncedDate = syncedDate }).ConfigureAwait(false);
            }
        }

        public void DeleteDraft(string returnOrderId)
        {
            if (string.IsNullOrWhiteSpace(returnOrderId))
                throw new ArgumentException("ReturnOrderId cannot be null or empty.", nameof(returnOrderId));

            using (var conn = CreateConnection())
            {
                conn.Open();

                const string checkSql = "SELECT StatusSync FROM BTRG_ReturnOrder WHERE ReturnOrderId = @ReturnOrderId;";
                var status = conn.QueryFirstOrDefault<string>(checkSql, new { ReturnOrderId = returnOrderId });
                if (status == null)
                    return;

                if (string.Equals(status, StatusSync.Synced.ToString(), StringComparison.OrdinalIgnoreCase))
                {
                    throw new DocumentLockedException(returnOrderId, "Delete");
                }

                using (var tran = conn.BeginTransaction())
                {
                    try
                    {
                        conn.Execute("DELETE FROM BTRG_ReturnOrderItem WHERE ReturnOrderId = @ReturnOrderId;", new { ReturnOrderId = returnOrderId }, tran);
                        conn.Execute("DELETE FROM BTRG_ReturnOrder WHERE ReturnOrderId = @ReturnOrderId;", new { ReturnOrderId = returnOrderId }, tran);
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

        public async Task DeleteDraftAsync(string returnOrderId)
        {
            if (string.IsNullOrWhiteSpace(returnOrderId))
                throw new ArgumentException("ReturnOrderId cannot be null or empty.", nameof(returnOrderId));

            using (var conn = CreateConnection())
            {
                await conn.OpenAsync().ConfigureAwait(false);

                const string checkSql = "SELECT StatusSync FROM BTRG_ReturnOrder WHERE ReturnOrderId = @ReturnOrderId;";
                var status = await conn.QueryFirstOrDefaultAsync<string>(checkSql, new { ReturnOrderId = returnOrderId }).ConfigureAwait(false);
                if (status == null)
                    return;

                if (string.Equals(status, StatusSync.Synced.ToString(), StringComparison.OrdinalIgnoreCase))
                {
                    throw new DocumentLockedException(returnOrderId, "Delete");
                }

                using (var tran = conn.BeginTransaction())
                {
                    try
                    {
                        await conn.ExecuteAsync("DELETE FROM BTRG_ReturnOrderItem WHERE ReturnOrderId = @ReturnOrderId;", new { ReturnOrderId = returnOrderId }, tran).ConfigureAwait(false);
                        await conn.ExecuteAsync("DELETE FROM BTRG_ReturnOrder WHERE ReturnOrderId = @ReturnOrderId;", new { ReturnOrderId = returnOrderId }, tran).ConfigureAwait(false);
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

        private static ReturnOrder MapToDomain(ReturnOrderHeaderRow header, List<ReturnOrderItemRow> items)
        {
            var status = Enum.TryParse<StatusSync>(header.StatusSync, true, out var s) ? s : StatusSync.Draft;
            var order = new ReturnOrder(
                header.ReturnOrderId,
                header.ReturnOrderDate,
                header.CustId,
                header.CustName,
                header.WarehouseCode,
                header.OperatorName,
                header.DriverId,
                header.DriverName,
                header.SalesPersonId,
                header.SalesPersonName,
                header.Keterangan,
                status,
                header.CreatedDate,
                header.ModifiedDate,
                header.SyncedDate);

            if (items != null && items.Any())
            {
                var domainItems = items.Select(i =>
                {
                    var jenis = Enum.TryParse<JenisRetur>(i.JenisRetur, true, out var j) ? j : JenisRetur.BAGUS;
                    return new ReturnOrderItem(
                        i.ReturnOrderId,
                        i.NoUrut,
                        i.BrgId,
                        i.BrgName,
                        i.Barcode,
                        jenis,
                        i.Qty,
                        i.SatId,
                        i.Keterangan);
                });
                order.LoadItems(domainItems);
            }

            return order;
        }

        private class ReturnOrderHeaderRow
        {
            public string ReturnOrderId { get; set; }
            public DateTime ReturnOrderDate { get; set; }
            public string CustId { get; set; }
            public string CustName { get; set; }
            public string DriverId { get; set; }
            public string DriverName { get; set; }
            public string SalesPersonId { get; set; }
            public string SalesPersonName { get; set; }
            public string WarehouseCode { get; set; }
            public string OperatorName { get; set; }
            public string Keterangan { get; set; }
            public string StatusSync { get; set; }
            public DateTime CreatedDate { get; set; }
            public DateTime ModifiedDate { get; set; }
            public DateTime? SyncedDate { get; set; }
        }

        private class ReturnOrderItemRow
        {
            public string ReturnOrderId { get; set; }
            public int NoUrut { get; set; }
            public string BrgId { get; set; }
            public string BrgName { get; set; }
            public string Barcode { get; set; }
            public string JenisRetur { get; set; }
            public decimal Qty { get; set; }
            public string SatId { get; set; }
            public string Keterangan { get; set; }
        }
    }
}
