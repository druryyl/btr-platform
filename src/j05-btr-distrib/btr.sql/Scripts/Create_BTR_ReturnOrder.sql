-- Return Order (S1.1): create BTR_ReturnOrder and BTR_ReturnOrderItem with their indexes.
-- Idempotent: skips objects that already exist; safe to re-run.
-- Source of truth: btr.sql/Tables/InventoryContext/BTR_ReturnOrder.sql,
--                  btr.sql/Tables/InventoryContext/BTR_ReturnOrderItem.sql
SET NOCOUNT ON;
GO

IF OBJECT_ID(N'dbo.BTR_ReturnOrder', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[BTR_ReturnOrder]
(
    ReturnOrderId   VARCHAR(26)  NOT NULL CONSTRAINT DF_BTR_ReturnOrder_ReturnOrderId   DEFAULT(''),
    ReturnOrderNo   VARCHAR(20)  NOT NULL CONSTRAINT DF_BTR_ReturnOrder_ReturnOrderNo   DEFAULT(''),
    ReturnOrderDate DATETIME     NOT NULL CONSTRAINT DF_BTR_ReturnOrder_ReturnOrderDate DEFAULT('3000-01-01'),
    WarehouseCode   VARCHAR(20)  NOT NULL CONSTRAINT DF_BTR_ReturnOrder_WarehouseCode   DEFAULT(''),
    CustomerId      VARCHAR(6)   NOT NULL CONSTRAINT DF_BTR_ReturnOrder_CustomerId      DEFAULT(''),
    SalesPersonId   VARCHAR(5)   NOT NULL CONSTRAINT DF_BTR_ReturnOrder_SalesPersonId   DEFAULT(''),
    DriverId        VARCHAR(5)   NOT NULL CONSTRAINT DF_BTR_ReturnOrder_DriverId        DEFAULT(''),
    Note            VARCHAR(100) NOT NULL CONSTRAINT DF_BTR_ReturnOrder_Note            DEFAULT(''),
    Status          VARCHAR(10)  NOT NULL CONSTRAINT DF_BTR_ReturnOrder_Status          DEFAULT('Synced'),

    CreatedBy       VARCHAR(50)  NOT NULL CONSTRAINT DF_BTR_ReturnOrder_CreatedBy       DEFAULT(''),
    CreatedDate     DATETIME     NOT NULL CONSTRAINT DF_BTR_ReturnOrder_CreatedDate     DEFAULT('3000-01-01'),
    ModifiedBy      VARCHAR(50)  NOT NULL CONSTRAINT DF_BTR_ReturnOrder_ModifiedBy      DEFAULT(''),
    ModifiedDate    DATETIME     NOT NULL CONSTRAINT DF_BTR_ReturnOrder_ModifiedDate    DEFAULT('3000-01-01'),

    RowVer          ROWVERSION   NOT NULL,

    CONSTRAINT PK_BTR_ReturnOrder PRIMARY KEY CLUSTERED (ReturnOrderId)
)
END
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = N'IX_BTR_ReturnOrder_Status'
      AND object_id = OBJECT_ID(N'dbo.BTR_ReturnOrder'))
BEGIN
CREATE INDEX IX_BTR_ReturnOrder_Status
    ON BTR_ReturnOrder(Status, ReturnOrderDate)
    WITH(FILLFACTOR=75)
END
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = N'IX_BTR_ReturnOrder_CustomerId'
      AND object_id = OBJECT_ID(N'dbo.BTR_ReturnOrder'))
BEGIN
CREATE INDEX IX_BTR_ReturnOrder_CustomerId
    ON BTR_ReturnOrder(CustomerId, ReturnOrderId)
    WITH(FILLFACTOR=75)
END
GO

IF OBJECT_ID(N'dbo.BTR_ReturnOrderItem', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[BTR_ReturnOrderItem]
(
    ReturnOrderId VARCHAR(26)     NOT NULL CONSTRAINT DF_BTR_ReturnOrderItem_ReturnOrderId DEFAULT(''),
    NoUrut        INT             NOT NULL CONSTRAINT DF_BTR_ReturnOrderItem_NoUrut        DEFAULT(0),
    BrgId         VARCHAR(6)      NOT NULL CONSTRAINT DF_BTR_ReturnOrderItem_BrgId         DEFAULT(''),
    BrgCode       VARCHAR(20)     NOT NULL CONSTRAINT DF_BTR_ReturnOrderItem_BrgCode       DEFAULT(''),
    Qty           DECIMAL(18,2)   NOT NULL CONSTRAINT DF_BTR_ReturnOrderItem_Qty           DEFAULT(0),
    SatId         VARCHAR(7)      NOT NULL CONSTRAINT DF_BTR_ReturnOrderItem_SatId         DEFAULT(''),
    JenisRetur    VARCHAR(5)      NOT NULL CONSTRAINT DF_BTR_ReturnOrderItem_JenisRetur    DEFAULT(''),

    CONSTRAINT PK_BTR_ReturnOrderItem PRIMARY KEY CLUSTERED (ReturnOrderId, NoUrut)
)
END
GO
