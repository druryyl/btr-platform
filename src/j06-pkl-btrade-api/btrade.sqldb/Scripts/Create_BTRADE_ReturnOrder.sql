-- Return Order (S2.1): create BTRADE_ReturnOrder and BTRADE_ReturnOrderItem with their indexes.
-- Idempotent: skips objects that already exist; safe to re-run.
-- Source of truth: btrade.sqldb/ReturnOrderContext/BTRADE_ReturnOrder.sql,
--                  btrade.sqldb/ReturnOrderContext/BTRADE_ReturnOrderItem.sql
SET NOCOUNT ON;
GO

IF OBJECT_ID(N'dbo.BTRADE_ReturnOrder', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[BTRADE_ReturnOrder]
(
    ReturnOrderId   VARCHAR(26)  NOT NULL CONSTRAINT DF_BTRADE_ReturnOrder_ReturnOrderId DEFAULT(''),
    ServerId        VARCHAR(5)   NOT NULL CONSTRAINT DF_BTRADE_ReturnOrder_ServerId      DEFAULT(''),
    ReturnOrderDate DATETIME     NOT NULL,
    WarehouseCode   VARCHAR(20)  NOT NULL CONSTRAINT DF_BTRADE_ReturnOrder_WarehouseCode DEFAULT(''),
    CustomerId      VARCHAR(6)   NOT NULL CONSTRAINT DF_BTRADE_ReturnOrder_CustomerId    DEFAULT(''),
    CustomerName    VARCHAR(100) NOT NULL CONSTRAINT DF_BTRADE_ReturnOrder_CustomerName  DEFAULT(''),
    SalesPersonId   VARCHAR(5)   NOT NULL CONSTRAINT DF_BTRADE_ReturnOrder_SalesPersonId DEFAULT(''),
    SalesPersonName VARCHAR(100) NOT NULL CONSTRAINT DF_BTRADE_ReturnOrder_SalesPersonName DEFAULT(''),
    DriverId        VARCHAR(5)   NOT NULL CONSTRAINT DF_BTRADE_ReturnOrder_DriverId      DEFAULT(''),
    DriverName      VARCHAR(20)  NOT NULL CONSTRAINT DF_BTRADE_ReturnOrder_DriverName    DEFAULT(''),
    Note            VARCHAR(100) NOT NULL CONSTRAINT DF_BTRADE_ReturnOrder_Note          DEFAULT(''),
    StatusSync      VARCHAR(10)  NOT NULL CONSTRAINT DF_BTRADE_ReturnOrder_StatusSync    DEFAULT('TERKIRIM'),

    CONSTRAINT PK_BTRADE_ReturnOrder PRIMARY KEY CLUSTERED (ReturnOrderId, ServerId)
)
END
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = N'IX_BTRADE_ReturnOrder_ServerId_Status'
      AND object_id = OBJECT_ID(N'dbo.BTRADE_ReturnOrder'))
BEGIN
CREATE INDEX IX_BTRADE_ReturnOrder_ServerId_Status
    ON [dbo].[BTRADE_ReturnOrder] (ServerId, StatusSync)
END
GO

IF OBJECT_ID(N'dbo.BTRADE_ReturnOrderItem', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[BTRADE_ReturnOrderItem]
(
    ReturnOrderId VARCHAR(26)   NOT NULL CONSTRAINT DF_BTRADE_ReturnOrderItem_ReturnOrderId DEFAULT(''),
    NoUrut        INT           NOT NULL CONSTRAINT DF_BTRADE_ReturnOrderItem_NoUrut        DEFAULT(0),
    BrgId         VARCHAR(6)    NOT NULL CONSTRAINT DF_BTRADE_ReturnOrderItem_BrgId         DEFAULT(''),
    BrgCode       VARCHAR(20)   NOT NULL CONSTRAINT DF_BTRADE_ReturnOrderItem_BrgCode       DEFAULT(''),
    BrgName       VARCHAR(60)   NOT NULL CONSTRAINT DF_BTRADE_ReturnOrderItem_BrgName       DEFAULT(''),
    Qty           DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTRADE_ReturnOrderItem_Qty           DEFAULT(0),
    SatId         VARCHAR(7)    NOT NULL CONSTRAINT DF_BTRADE_ReturnOrderItem_SatId         DEFAULT(''),
    JenisRetur    VARCHAR(5)    NOT NULL CONSTRAINT DF_BTRADE_ReturnOrderItem_JenisRetur    DEFAULT(''),

    CONSTRAINT PK_BTRADE_ReturnOrderItem PRIMARY KEY CLUSTERED (ReturnOrderId, NoUrut)
)
END
GO
