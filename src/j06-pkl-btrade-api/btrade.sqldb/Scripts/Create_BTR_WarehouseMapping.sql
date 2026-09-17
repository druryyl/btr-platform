-- Return Order (S2.4): create BTR_WarehouseMapping (WarehouseCode -> ServerId) and seed it.
-- Idempotent: skips objects that already exist and rows already seeded; safe to re-run.
-- Source of truth: btrade.sqldb/WarehouseContext/BTR_WarehouseMapping.sql
-- Seed mirrors the initial Warehouse Mapping rows (Architecture §6.3).
SET NOCOUNT ON;
GO

IF OBJECT_ID(N'dbo.BTR_WarehouseMapping', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[BTR_WarehouseMapping]
(
    WarehouseCode VARCHAR(20) NOT NULL CONSTRAINT DF_BTR_WarehouseMapping_WarehouseCode DEFAULT(''),
    ServerId      VARCHAR(5)  NOT NULL CONSTRAINT DF_BTR_WarehouseMapping_ServerId      DEFAULT(''),

    CONSTRAINT PK_BTR_WarehouseMapping PRIMARY KEY CLUSTERED (WarehouseCode)
)
END
GO

IF NOT EXISTS (SELECT 1 FROM BTR_WarehouseMapping WHERE WarehouseCode = 'GAMPING')
    INSERT INTO BTR_WarehouseMapping (WarehouseCode, ServerId) VALUES ('GAMPING', 'JOGJA');
GO

IF NOT EXISTS (SELECT 1 FROM BTR_WarehouseMapping WHERE WarehouseCode = 'CONCAT')
    INSERT INTO BTR_WarehouseMapping (WarehouseCode, ServerId) VALUES ('CONCAT', 'JOGJA');
GO

IF NOT EXISTS (SELECT 1 FROM BTR_WarehouseMapping WHERE WarehouseCode = 'MAGELANG')
    INSERT INTO BTR_WarehouseMapping (WarehouseCode, ServerId) VALUES ('MAGELANG', 'MGL');
GO
