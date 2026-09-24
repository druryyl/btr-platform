-- create table for ReturnOrderType (Cloud relay)
CREATE TABLE BTRADE_ReturnOrder(
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
    SubmittedBy     VARCHAR(50)  NOT NULL CONSTRAINT DF_BTRADE_ReturnOrder_SubmittedBy DEFAULT(''),

    CONSTRAINT PK_BTRADE_ReturnOrder PRIMARY KEY CLUSTERED (ReturnOrderId, ServerId)
)
GO

CREATE INDEX IX_BTRADE_ReturnOrder_ServerId_Status
    ON BTRADE_ReturnOrder(ServerId, StatusSync)
GO
