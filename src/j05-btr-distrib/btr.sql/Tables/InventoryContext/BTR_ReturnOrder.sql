-- create table for ReturnOrderModel
CREATE TABLE BTR_ReturnOrder(
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
GO

CREATE INDEX IX_BTR_ReturnOrder_Status
    ON BTR_ReturnOrder(Status, ReturnOrderDate)
    WITH(FILLFACTOR=75)
GO

CREATE INDEX IX_BTR_ReturnOrder_CustomerId
    ON BTR_ReturnOrder(CustomerId, ReturnOrderId)
    WITH(FILLFACTOR=75)
GO
