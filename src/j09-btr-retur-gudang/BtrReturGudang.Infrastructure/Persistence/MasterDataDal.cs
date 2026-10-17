using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using BtrReturGudang.AppTier.Repositories;
using BtrReturGudang.Domain.Entities;
using BtrReturGudang.Infrastructure.Helpers;
using Dapper;

namespace BtrReturGudang.Infrastructure.Persistence
{
    /// <summary>
    /// Dapper persistence implementation for IReferenceRepo managing master/reference data caches
    /// (Barang, Customer, Driver, SalesPerson) against local SQL Server (btrgd).
    /// Traceable to: ARCHITECTURE §5.1, §8, P2-S04.
    /// </summary>
    public class MasterDataDal : IReferenceRepo
    {
        private readonly string _connectionString;

        public MasterDataDal(string connectionString = null)
        {
            _connectionString = connectionString;
        }

        private SqlConnection CreateConnection() => ConnStringHelper.CreateConnection(_connectionString);

        #region Barang

        public void BulkUpsertBarang(IEnumerable<Barang> items)
        {
            if (items == null) return;
            var list = items.ToList();
            if (!list.Any()) return;

            using (var conn = CreateConnection())
            {
                conn.Open();
                using (var tran = conn.BeginTransaction())
                {
                    try
                    {
                        const string sql = @"
IF EXISTS (SELECT 1 FROM BTRG_Barang WHERE BrgId = @BrgId)
BEGIN
    UPDATE BTRG_Barang
    SET BrgName = @BrgName,
        Satuan1 = @Satuan1,
        Satuan2 = @Satuan2,
        Satuan3 = @Satuan3,
        IsActive = @IsActive
    WHERE BrgId = @BrgId;
END
ELSE
BEGIN
    INSERT INTO BTRG_Barang (BrgId, BrgName, Satuan1, Satuan2, Satuan3, IsActive)
    VALUES (@BrgId, @BrgName, @Satuan1, @Satuan2, @Satuan3, @IsActive);
END;";

                        conn.Execute(sql, list, tran);
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

        public async Task BulkUpsertBarangAsync(IEnumerable<Barang> items)
        {
            if (items == null) return;
            var list = items.ToList();
            if (!list.Any()) return;

            using (var conn = CreateConnection())
            {
                await conn.OpenAsync().ConfigureAwait(false);
                using (var tran = conn.BeginTransaction())
                {
                    try
                    {
                        const string sql = @"
IF EXISTS (SELECT 1 FROM BTRG_Barang WHERE BrgId = @BrgId)
BEGIN
    UPDATE BTRG_Barang
    SET BrgName = @BrgName,
        Satuan1 = @Satuan1,
        Satuan2 = @Satuan2,
        Satuan3 = @Satuan3,
        IsActive = @IsActive
    WHERE BrgId = @BrgId;
END
ELSE
BEGIN
    INSERT INTO BTRG_Barang (BrgId, BrgName, Satuan1, Satuan2, Satuan3, IsActive)
    VALUES (@BrgId, @BrgName, @Satuan1, @Satuan2, @Satuan3, @IsActive);
END;";

                        await conn.ExecuteAsync(sql, list, tran).ConfigureAwait(false);
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

        public IEnumerable<Barang> SearchBarangByNameOrCode(string keyword)
        {
            var searchKey = string.IsNullOrWhiteSpace(keyword) ? "%" : $"%{keyword.Trim()}%";
            using (var conn = CreateConnection())
            {
                conn.Open();
                const string sql = @"
SELECT BrgId, BrgName, Satuan1, Satuan2, Satuan3, IsActive
FROM BTRG_Barang
WHERE (BrgId LIKE @Keyword OR BrgName LIKE @Keyword) AND IsActive = 1
ORDER BY BrgName;";

                var rows = conn.Query<BarangRow>(sql, new { Keyword = searchKey });
                return rows.Select(MapToBarang).ToList();
            }
        }

        public async Task<IEnumerable<Barang>> SearchBarangByNameOrCodeAsync(string keyword)
        {
            var searchKey = string.IsNullOrWhiteSpace(keyword) ? "%" : $"%{keyword.Trim()}%";
            using (var conn = CreateConnection())
            {
                await conn.OpenAsync().ConfigureAwait(false);
                const string sql = @"
SELECT BrgId, BrgName, Satuan1, Satuan2, Satuan3, IsActive
FROM BTRG_Barang
WHERE (BrgId LIKE @Keyword OR BrgName LIKE @Keyword) AND IsActive = 1
ORDER BY BrgName;";

                var rows = await conn.QueryAsync<BarangRow>(sql, new { Keyword = searchKey }).ConfigureAwait(false);
                return rows.Select(MapToBarang).ToList();
            }
        }

        public Barang GetBarangById(string brgId)
        {
            if (string.IsNullOrWhiteSpace(brgId)) return null;

            using (var conn = CreateConnection())
            {
                conn.Open();
                const string sql = @"
SELECT BrgId, BrgName, Satuan1, Satuan2, Satuan3, IsActive
FROM BTRG_Barang
WHERE BrgId = @BrgId;";

                var row = conn.QueryFirstOrDefault<BarangRow>(sql, new { BrgId = brgId });
                return row != null ? MapToBarang(row) : null;
            }
        }

        public async Task<Barang> GetBarangByIdAsync(string brgId)
        {
            if (string.IsNullOrWhiteSpace(brgId)) return null;

            using (var conn = CreateConnection())
            {
                await conn.OpenAsync().ConfigureAwait(false);
                const string sql = @"
SELECT BrgId, BrgName, Satuan1, Satuan2, Satuan3, IsActive
FROM BTRG_Barang
WHERE BrgId = @BrgId;";

                var row = await conn.QueryFirstOrDefaultAsync<BarangRow>(sql, new { BrgId = brgId }).ConfigureAwait(false);
                return row != null ? MapToBarang(row) : null;
            }
        }

        #endregion

        #region Customer

        public void BulkUpsertCustomer(IEnumerable<Customer> customers)
        {
            if (customers == null) return;
            var list = customers.ToList();
            if (!list.Any()) return;

            using (var conn = CreateConnection())
            {
                conn.Open();
                using (var tran = conn.BeginTransaction())
                {
                    try
                    {
                        const string sql = @"
IF EXISTS (SELECT 1 FROM BTRG_Customer WHERE CustomerId = @CustomerId)
BEGIN
    UPDATE BTRG_Customer
    SET CustomerCode = @CustomerCode,
        CustomerName = @CustomerName,
        Alamat = @Alamat,
        Wilayah = @Wilayah
    WHERE CustomerId = @CustomerId;
END
ELSE
BEGIN
    INSERT INTO BTRG_Customer (CustomerId, CustomerCode, CustomerName, Alamat, Wilayah)
    VALUES (@CustomerId, @CustomerCode, @CustomerName, @Alamat, @Wilayah);
END;";

                        conn.Execute(sql, list, tran);
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

        public async Task BulkUpsertCustomerAsync(IEnumerable<Customer> customers)
        {
            if (customers == null) return;
            var list = customers.ToList();
            if (!list.Any()) return;

            using (var conn = CreateConnection())
            {
                await conn.OpenAsync().ConfigureAwait(false);
                using (var tran = conn.BeginTransaction())
                {
                    try
                    {
                        const string sql = @"
IF EXISTS (SELECT 1 FROM BTRG_Customer WHERE CustomerId = @CustomerId)
BEGIN
    UPDATE BTRG_Customer
    SET CustomerCode = @CustomerCode,
        CustomerName = @CustomerName,
        Alamat = @Alamat,
        Wilayah = @Wilayah
    WHERE CustomerId = @CustomerId;
END
ELSE
BEGIN
    INSERT INTO BTRG_Customer (CustomerId, CustomerCode, CustomerName, Alamat, Wilayah)
    VALUES (@CustomerId, @CustomerCode, @CustomerName, @Alamat, @Wilayah);
END;";

                        await conn.ExecuteAsync(sql, list, tran).ConfigureAwait(false);
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

        public IEnumerable<Customer> SearchCustomer(string keyword)
        {
            var searchKey = string.IsNullOrWhiteSpace(keyword) ? "%" : $"%{keyword.Trim()}%";
            using (var conn = CreateConnection())
            {
                conn.Open();
                const string sql = @"
SELECT CustomerId, CustomerCode, CustomerName, Alamat, Wilayah
FROM BTRG_Customer
WHERE CustomerId LIKE @Keyword 
   OR CustomerCode LIKE @Keyword 
   OR CustomerName LIKE @Keyword 
   OR Wilayah LIKE @Keyword
ORDER BY CustomerName;";

                var rows = conn.Query<CustomerRow>(sql, new { Keyword = searchKey });
                return rows.Select(MapToCustomer).ToList();
            }
        }

        public async Task<IEnumerable<Customer>> SearchCustomerAsync(string keyword)
        {
            var searchKey = string.IsNullOrWhiteSpace(keyword) ? "%" : $"%{keyword.Trim()}%";
            using (var conn = CreateConnection())
            {
                await conn.OpenAsync().ConfigureAwait(false);
                const string sql = @"
SELECT CustomerId, CustomerCode, CustomerName, Alamat, Wilayah
FROM BTRG_Customer
WHERE CustomerId LIKE @Keyword 
   OR CustomerCode LIKE @Keyword 
   OR CustomerName LIKE @Keyword 
   OR Wilayah LIKE @Keyword
ORDER BY CustomerName;";

                var rows = await conn.QueryAsync<CustomerRow>(sql, new { Keyword = searchKey }).ConfigureAwait(false);
                return rows.Select(MapToCustomer).ToList();
            }
        }

        public Customer GetCustomerById(string custId)
        {
            if (string.IsNullOrWhiteSpace(custId)) return null;

            using (var conn = CreateConnection())
            {
                conn.Open();
                const string sql = @"
SELECT CustomerId, CustomerCode, CustomerName, Alamat, Wilayah
FROM BTRG_Customer
WHERE CustomerId = @CustomerId;";

                var row = conn.QueryFirstOrDefault<CustomerRow>(sql, new { CustomerId = custId });
                return row != null ? MapToCustomer(row) : null;
            }
        }

        public async Task<Customer> GetCustomerByIdAsync(string custId)
        {
            if (string.IsNullOrWhiteSpace(custId)) return null;

            using (var conn = CreateConnection())
            {
                await conn.OpenAsync().ConfigureAwait(false);
                const string sql = @"
SELECT CustomerId, CustomerCode, CustomerName, Alamat, Wilayah
FROM BTRG_Customer
WHERE CustomerId = @CustomerId;";

                var row = await conn.QueryFirstOrDefaultAsync<CustomerRow>(sql, new { CustomerId = custId }).ConfigureAwait(false);
                return row != null ? MapToCustomer(row) : null;
            }
        }

        #endregion

        #region Driver

        public void BulkUpsertDriver(IEnumerable<Driver> drivers)
        {
            if (drivers == null) return;
            var list = drivers.ToList();
            if (!list.Any()) return;

            using (var conn = CreateConnection())
            {
                conn.Open();
                using (var tran = conn.BeginTransaction())
                {
                    try
                    {
                        const string sql = @"
IF EXISTS (SELECT 1 FROM BTRG_Driver WHERE DriverId = @DriverId)
BEGIN
    UPDATE BTRG_Driver
    SET DriverName = @DriverName
    WHERE DriverId = @DriverId;
END
ELSE
BEGIN
    INSERT INTO BTRG_Driver (DriverId, DriverName)
    VALUES (@DriverId, @DriverName);
END;";

                        conn.Execute(sql, list, tran);
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

        public async Task BulkUpsertDriverAsync(IEnumerable<Driver> drivers)
        {
            if (drivers == null) return;
            var list = drivers.ToList();
            if (!list.Any()) return;

            using (var conn = CreateConnection())
            {
                await conn.OpenAsync().ConfigureAwait(false);
                using (var tran = conn.BeginTransaction())
                {
                    try
                    {
                        const string sql = @"
IF EXISTS (SELECT 1 FROM BTRG_Driver WHERE DriverId = @DriverId)
BEGIN
    UPDATE BTRG_Driver
    SET DriverName = @DriverName
    WHERE DriverId = @DriverId;
END
ELSE
BEGIN
    INSERT INTO BTRG_Driver (DriverId, DriverName)
    VALUES (@DriverId, @DriverName);
END;";

                        await conn.ExecuteAsync(sql, list, tran).ConfigureAwait(false);
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

        public IEnumerable<Driver> GetAllDrivers()
        {
            using (var conn = CreateConnection())
            {
                conn.Open();
                const string sql = @"
SELECT DriverId, DriverName
FROM BTRG_Driver
ORDER BY DriverName;";

                var rows = conn.Query<DriverRow>(sql);
                return rows.Select(MapToDriver).ToList();
            }
        }

        public async Task<IEnumerable<Driver>> GetAllDriversAsync()
        {
            using (var conn = CreateConnection())
            {
                await conn.OpenAsync().ConfigureAwait(false);
                const string sql = @"
SELECT DriverId, DriverName
FROM BTRG_Driver
ORDER BY DriverName;";

                var rows = await conn.QueryAsync<DriverRow>(sql).ConfigureAwait(false);
                return rows.Select(MapToDriver).ToList();
            }
        }

        #endregion

        #region SalesPerson

        public void BulkUpsertSalesPerson(IEnumerable<SalesPerson> salesPersons)
        {
            if (salesPersons == null) return;
            var list = salesPersons.ToList();
            if (!list.Any()) return;

            using (var conn = CreateConnection())
            {
                conn.Open();
                using (var tran = conn.BeginTransaction())
                {
                    try
                    {
                        const string sql = @"
IF EXISTS (SELECT 1 FROM BTRG_SalesPerson WHERE SalesPersonId = @SalesPersonId)
BEGIN
    UPDATE BTRG_SalesPerson
    SET SalesPersonName = @SalesPersonName
    WHERE SalesPersonId = @SalesPersonId;
END
ELSE
BEGIN
    INSERT INTO BTRG_SalesPerson (SalesPersonId, SalesPersonName)
    VALUES (@SalesPersonId, @SalesPersonName);
END;";

                        conn.Execute(sql, list, tran);
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

        public async Task BulkUpsertSalesPersonAsync(IEnumerable<SalesPerson> salesPersons)
        {
            if (salesPersons == null) return;
            var list = salesPersons.ToList();
            if (!list.Any()) return;

            using (var conn = CreateConnection())
            {
                await conn.OpenAsync().ConfigureAwait(false);
                using (var tran = conn.BeginTransaction())
                {
                    try
                    {
                        const string sql = @"
IF EXISTS (SELECT 1 FROM BTRG_SalesPerson WHERE SalesPersonId = @SalesPersonId)
BEGIN
    UPDATE BTRG_SalesPerson
    SET SalesPersonName = @SalesPersonName
    WHERE SalesPersonId = @SalesPersonId;
END
ELSE
BEGIN
    INSERT INTO BTRG_SalesPerson (SalesPersonId, SalesPersonName)
    VALUES (@SalesPersonId, @SalesPersonName);
END;";

                        await conn.ExecuteAsync(sql, list, tran).ConfigureAwait(false);
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

        public IEnumerable<SalesPerson> GetAllSalesPersons()
        {
            using (var conn = CreateConnection())
            {
                conn.Open();
                const string sql = @"
SELECT SalesPersonId, SalesPersonName
FROM BTRG_SalesPerson
ORDER BY SalesPersonName;";

                var rows = conn.Query<SalesPersonRow>(sql);
                return rows.Select(MapToSalesPerson).ToList();
            }
        }

        public async Task<IEnumerable<SalesPerson>> GetAllSalesPersonsAsync()
        {
            using (var conn = CreateConnection())
            {
                await conn.OpenAsync().ConfigureAwait(false);
                const string sql = @"
SELECT SalesPersonId, SalesPersonName
FROM BTRG_SalesPerson
ORDER BY SalesPersonName;";

                var rows = await conn.QueryAsync<SalesPersonRow>(sql).ConfigureAwait(false);
                return rows.Select(MapToSalesPerson).ToList();
            }
        }

        #endregion

        #region Private Mappers and Row POCOs

        private static Barang MapToBarang(BarangRow row)
        {
            return new Barang(row.BrgId, row.BrgName, row.Satuan1, row.Satuan2, row.Satuan3, row.IsActive);
        }

        private static Customer MapToCustomer(CustomerRow row)
        {
            return new Customer(
                row.CustomerId ?? row.CustId,
                row.CustomerName ?? row.CustName,
                row.Alamat,
                row.CustomerCode ?? row.CustCode,
                row.Wilayah);
        }

        private static Driver MapToDriver(DriverRow row)
        {
            return new Driver(row.DriverId, row.DriverName);
        }

        private static SalesPerson MapToSalesPerson(SalesPersonRow row)
        {
            return new SalesPerson(row.SalesPersonId, row.SalesPersonName);
        }

        private class BarangRow
        {
            public string BrgId { get; set; }
            public string BrgName { get; set; }
            public string Satuan1 { get; set; }
            public string Satuan2 { get; set; }
            public string Satuan3 { get; set; }
            public bool IsActive { get; set; }
        }

        private class CustomerRow
        {
            public string CustomerId { get; set; }
            public string CustomerCode { get; set; }
            public string CustomerName { get; set; }
            public string Alamat { get; set; }
            public string Wilayah { get; set; }

            public string CustId { get => CustomerId; set => CustomerId = value; }
            public string CustName { get => CustomerName; set => CustomerName = value; }
            public string CustCode { get => CustomerCode; set => CustomerCode = value; }
        }

        private class DriverRow
        {
            public string DriverId { get; set; }
            public string DriverName { get; set; }
        }

        private class SalesPersonRow
        {
            public string SalesPersonId { get; set; }
            public string SalesPersonName { get; set; }
        }

        #endregion
    }
}
