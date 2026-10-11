-- ============================================================================
-- Script: 001_CreateWarehouseTables.sql
-- Description: Create all warehouse tables, primary keys, and indexes for btrgd.
-- Adheres strictly to IR-RO-11: Zero database foreign keys declared.
-- ============================================================================

-- 1. BTRG_ReturnOrder
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'BTRG_ReturnOrder' AND type = 'U')
BEGIN
    CREATE TABLE BTRG_ReturnOrder (
        ReturnOrderId    VARCHAR(26)   NOT NULL PRIMARY KEY, -- ULID
        ReturnOrderDate  DATE          NOT NULL,
        CustId           VARCHAR(10)   NOT NULL,
        CustName         VARCHAR(100)  NOT NULL,
        DriverId         VARCHAR(10)   NULL,
        DriverName       VARCHAR(100)  NULL,
        SalesPersonId    VARCHAR(10)   NULL,
        SalesPersonName  VARCHAR(100)  NULL,
        WarehouseCode    VARCHAR(20)   NOT NULL,
        OperatorName     VARCHAR(50)   NOT NULL,
        Keterangan       VARCHAR(250)  NULL,
        StatusSync       VARCHAR(10)   NOT NULL CONSTRAINT DF_BTRG_ReturnOrder_StatusSync DEFAULT 'Draft', -- 'Draft' or 'Synced'
        CreatedDate      DATETIME      NOT NULL CONSTRAINT DF_BTRG_ReturnOrder_CreatedDate DEFAULT GETDATE(),
        ModifiedDate     DATETIME      NOT NULL CONSTRAINT DF_BTRG_ReturnOrder_ModifiedDate DEFAULT GETDATE(),
        SyncedDate       DATETIME      NULL
    );
END;

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_BTRG_ReturnOrder_StatusSync' AND object_id = OBJECT_ID('BTRG_ReturnOrder'))
BEGIN
    CREATE INDEX IX_BTRG_ReturnOrder_StatusSync ON BTRG_ReturnOrder (StatusSync, ReturnOrderDate);
END;

-- 2. BTRG_ReturnOrderItem
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'BTRG_ReturnOrderItem' AND type = 'U')
BEGIN
    CREATE TABLE BTRG_ReturnOrderItem (
        ReturnOrderId    VARCHAR(26)   NOT NULL,
        NoUrut           INT           NOT NULL,
        BrgId            VARCHAR(20)   NOT NULL,
        BrgName          VARCHAR(100)  NOT NULL,
        Barcode          VARCHAR(50)   NULL,
        JenisRetur       VARCHAR(5)    NOT NULL, -- 'BAGUS' or 'RUSAK'
        Qty              DECIMAL(18,2) NOT NULL,
        SatId            VARCHAR(7)    NOT NULL, -- Packaging unit (Dus, Pak, Pcs)
        Keterangan       VARCHAR(250)  NULL,
        CONSTRAINT PK_BTRG_ReturnOrderItem PRIMARY KEY (ReturnOrderId, NoUrut)
    );
END;

-- 3. BTRG_Barcode
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'BTRG_Barcode' AND type = 'U')
BEGIN
    CREATE TABLE BTRG_Barcode (
        Barcode          VARCHAR(50)   NOT NULL PRIMARY KEY,
        BrgId            VARCHAR(20)   NOT NULL,
        BrgName          VARCHAR(100)  NOT NULL,
        Satuan           VARCHAR(7)    NOT NULL,
        Pengali          DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTRG_Barcode_Pengali DEFAULT 1.0,
        IsActive         BIT           NOT NULL CONSTRAINT DF_BTRG_Barcode_IsActive DEFAULT 1
    );
END;

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_BTRG_Barcode_BrgId' AND object_id = OBJECT_ID('BTRG_Barcode'))
BEGIN
    CREATE INDEX IX_BTRG_Barcode_BrgId ON BTRG_Barcode (BrgId);
END;

-- 4. BTRG_BarcodeRegistration
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'BTRG_BarcodeRegistration' AND type = 'U')
BEGIN
    CREATE TABLE BTRG_BarcodeRegistration (
        RequestId        VARCHAR(26)   NOT NULL PRIMARY KEY, -- ULID
        Barcode          VARCHAR(50)   NOT NULL,
        BrgId            VARCHAR(20)   NOT NULL,
        Satuan           VARCHAR(7)    NOT NULL,
        WarehouseCode    VARCHAR(20)   NOT NULL,
        OperatorName     VARCHAR(50)   NOT NULL,
        StatusSync       VARCHAR(10)   NOT NULL CONSTRAINT DF_BTRG_BarcodeRegistration_StatusSync DEFAULT 'Pending', -- 'Pending' or 'Uploaded'
        CreatedDate      DATETIME      NOT NULL CONSTRAINT DF_BTRG_BarcodeRegistration_CreatedDate DEFAULT GETDATE()
    );
END;

-- 5. BTRG_Barang (Master Cache)
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'BTRG_Barang' AND type = 'U')
BEGIN
    CREATE TABLE BTRG_Barang (
        BrgId            VARCHAR(20)   NOT NULL PRIMARY KEY,
        BrgName          VARCHAR(100)  NOT NULL,
        Satuan1          VARCHAR(7)    NULL,
        Satuan2          VARCHAR(7)    NULL,
        Satuan3          VARCHAR(7)    NULL,
        IsActive         BIT           NOT NULL CONSTRAINT DF_BTRG_Barang_IsActive DEFAULT 1
    );
END;

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_BTRG_Barang_BrgName' AND object_id = OBJECT_ID('BTRG_Barang'))
BEGIN
    CREATE INDEX IX_BTRG_Barang_BrgName ON BTRG_Barang (BrgName);
END;

-- 6. BTRG_Customer (Reference Cache)
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'BTRG_Customer' AND type = 'U')
BEGIN
    CREATE TABLE BTRG_Customer (
        CustId           VARCHAR(10)   NOT NULL PRIMARY KEY,
        CustName         VARCHAR(100)  NOT NULL,
        Alamat           VARCHAR(150)  NULL
    );
END;

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_BTRG_Customer_Name' AND object_id = OBJECT_ID('BTRG_Customer'))
BEGIN
    CREATE INDEX IX_BTRG_Customer_Name ON BTRG_Customer (CustName);
END;

-- 7. BTRG_Driver (Reference Cache)
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'BTRG_Driver' AND type = 'U')
BEGIN
    CREATE TABLE BTRG_Driver (
        DriverId         VARCHAR(10)   NOT NULL PRIMARY KEY,
        DriverName       VARCHAR(100)  NOT NULL
    );
END;

-- 8. BTRG_SalesPerson (Reference Cache)
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'BTRG_SalesPerson' AND type = 'U')
BEGIN
    CREATE TABLE BTRG_SalesPerson (
        SalesPersonId    VARCHAR(10)   NOT NULL PRIMARY KEY,
        SalesPersonName  VARCHAR(100)  NOT NULL
    );
END;

-- 9. BTRG_SyncLog (Execution & Result Log)
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'BTRG_SyncLog' AND type = 'U')
BEGIN
    CREATE TABLE BTRG_SyncLog (
        SyncLogId        VARCHAR(26)   NOT NULL PRIMARY KEY, -- ULID
        SyncTimestamp    DATETIME      NOT NULL CONSTRAINT DF_BTRG_SyncLog_SyncTimestamp DEFAULT GETDATE(),
        SyncType         VARCHAR(20)   NOT NULL, -- 'ReturnOrder', 'Barcode', 'MasterData'
        Status           VARCHAR(10)   NOT NULL, -- 'SUCCESS', 'FAILED'
        RecordsPushed    INT           NOT NULL CONSTRAINT DF_BTRG_SyncLog_RecordsPushed DEFAULT 0,
        RecordsPulled    INT           NOT NULL CONSTRAINT DF_BTRG_SyncLog_RecordsPulled DEFAULT 0,
        ErrorMessage     VARCHAR(500)  NULL
    );
END;

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_BTRG_SyncLog_Timestamp' AND object_id = OBJECT_ID('BTRG_SyncLog'))
BEGIN
    CREATE INDEX IX_BTRG_SyncLog_Timestamp ON BTRG_SyncLog (SyncTimestamp);
END;
