-- ============================================================================
-- Script: 002_UpdateCustomerAndReturnOrderColumns.sql
-- Description: Upgrade BTRG_Customer and BTRG_ReturnOrder tables to persist
--              CustomerId, CustomerCode, CustomerName, Alamat, and Wilayah.
-- ============================================================================

-- 1. Upgrade BTRG_Customer
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'BTRG_Customer' AND type = 'U')
BEGIN
    -- Rename CustId -> CustomerId if legacy column exists
    IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('BTRG_Customer') AND name = 'CustId')
       AND NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('BTRG_Customer') AND name = 'CustomerId')
    BEGIN
        EXEC sp_rename 'BTRG_Customer.CustId', 'CustomerId', 'COLUMN';
    END;

    -- Rename CustName -> CustomerName if legacy column exists
    IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('BTRG_Customer') AND name = 'CustName')
       AND NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('BTRG_Customer') AND name = 'CustomerName')
    BEGIN
        EXEC sp_rename 'BTRG_Customer.CustName', 'CustomerName', 'COLUMN';
    END;

    -- Add CustomerCode if not present
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('BTRG_Customer') AND name = 'CustomerCode')
    BEGIN
        ALTER TABLE BTRG_Customer ADD CustomerCode VARCHAR(20) NULL;
    END;

    -- Add Wilayah if not present
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('BTRG_Customer') AND name = 'Wilayah')
    BEGIN
        ALTER TABLE BTRG_Customer ADD Wilayah VARCHAR(100) NULL;
    END;

    -- Widen Alamat to VARCHAR(250)
    IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('BTRG_Customer') AND name = 'Alamat' AND max_length < 250)
    BEGIN
        ALTER TABLE BTRG_Customer ALTER COLUMN Alamat VARCHAR(250) NULL;
    END;

    -- Widen CustomerId to VARCHAR(20) if needed
    IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('BTRG_Customer') AND name = 'CustomerId' AND max_length < 20)
    BEGIN
        ALTER TABLE BTRG_Customer ALTER COLUMN CustomerId VARCHAR(20) NOT NULL;
    END;
END;

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_BTRG_Customer_Code' AND object_id = OBJECT_ID('BTRG_Customer'))
BEGIN
    CREATE INDEX IX_BTRG_Customer_Code ON BTRG_Customer (CustomerCode);
END;

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_BTRG_Customer_Name' AND object_id = OBJECT_ID('BTRG_Customer'))
BEGIN
    CREATE INDEX IX_BTRG_Customer_Name ON BTRG_Customer (CustomerName);
END;

-- 2. Upgrade BTRG_ReturnOrder
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'BTRG_ReturnOrder' AND type = 'U')
BEGIN
    -- Add CustomerCode if not present
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('BTRG_ReturnOrder') AND name = 'CustomerCode')
    BEGIN
        ALTER TABLE BTRG_ReturnOrder ADD CustomerCode VARCHAR(20) NULL;
    END;

    -- Add Wilayah if not present
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('BTRG_ReturnOrder') AND name = 'Wilayah')
    BEGIN
        ALTER TABLE BTRG_ReturnOrder ADD Wilayah VARCHAR(100) NULL;
    END;
END;
