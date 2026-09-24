-- BTR_WarehouseMapping Table Creation Script
-- BGud Google Sign-In - Location to ServerId Mapping
-- This table stores the mapping between warehouse location IDs and server IDs
-- Used by Cloud SessionContextResolver to resolve locationId to ServerId
--
-- Schema: Main Office (btr2) / Cloud (btrade.sqldb)
-- Referenced in: BGUD-GOOGLE-SIGNIN-ARCHITECTURE.md §8 Migration Considerations
--                 BGUD-GOOGLE-SIGNIN-ARCHITECTURE.md Section 4. TD-04
--                 BGUD-GOOGLE-SIGNIN-FEASIBILITY-ASSESSMENT.md §2 Current State
--

IF OBJECT_ID('BTR_WarehouseMapping', 'U') IS NULL
BEGIN
    CREATE TABLE BTR_WarehouseMapping
    (
        WarehouseCode    VARCHAR(50)  NOT NULL,
        ServerId         VARCHAR(50)  NOT NULL,
        Description      VARCHAR(250) NULL,
        IsActive         BIT          NOT NULL CONSTRAINT DF_BTR_WarehouseMapping_IsActive DEFAULT (1),
        CreatedDate      DATETIME     NOT NULL CONSTRAINT DF_BTR_WarehouseMapping_CreatedDate DEFAULT (GETDATE()),
        ModifiedDate     DATETIME     NOT NULL CONSTRAINT DF_BTR_WarehouseMapping_ModifiedDate DEFAULT (GETDATE()),
        
        CONSTRAINT PK_BTR_WarehouseMapping PRIMARY KEY (WarehouseCode)
    );
END
GO

-- Create indexes for performance
IF NOT EXISTS (
    SELECT * FROM sys.indexes 
    WHERE object_id = OBJECT_ID('BTR_WarehouseMapping') 
    AND name = 'IX_BTR_WarehouseMapping_ServerId'
)
BEGIN
    CREATE INDEX IX_BTR_WarehouseMapping_ServerId 
    ON BTR_WarehouseMapping(ServerId);
END
GO

-- Insert BGud location mappings
-- These mappings link BGud warehouse location IDs to BTR server IDs
-- GAMPING and CONCAT both map to JOGJA (same server)
-- MAGELANG maps to MGL

IF NOT EXISTS (
    SELECT 1 FROM BTR_WarehouseMapping WHERE WarehouseCode = 'GAMPING'
)
BEGIN
    INSERT INTO BTR_WarehouseMapping (WarehouseCode, ServerId, Description)
    VALUES ('GAMPING', 'JOGJA', 'Gudang Gamping - Warehouse Location');
END
GO

IF NOT EXISTS (
    SELECT 1 FROM BTR_WarehouseMapping WHERE WarehouseCode = 'CONCAT'
)
BEGIN
    INSERT INTO BTR_WarehouseMapping (WarehouseCode, ServerId, Description)
    VALUES ('CONCAT', 'JOGJA', 'Gudang Concat - Warehouse Location');
END
GO

IF NOT EXISTS (
    SELECT 1 FROM BTR_WarehouseMapping WHERE WarehouseCode = 'MAGELANG'
)
BEGIN
    INSERT INTO BTR_WarehouseMapping (WarehouseCode, ServerId, Description)
    VALUES ('MAGELANG', 'MGL', 'Gudang Magelang - Warehouse Location');
END
GO

-- Verification query
SELECT 
    WarehouseCode,
    ServerId,
    Description,
    IsActive
FROM BTR_WarehouseMapping
WHERE IsActive = 1
ORDER BY ServerId, WarehouseCode;
GO

-- Note: This table is typically seeded/maintained in the Cloud database (btrade.sqldb)
-- The Main Office may have its own warehouse reference tables, but the location mapping
-- for BGud's session context resolution is authoritative in the Cloud.
-- See ARCHITECTURE.md §8 Migration Considerations for coordinated deployment notes.
