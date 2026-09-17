-- create table for ReturnOrderItemType (Cloud relay)
CREATE TABLE BTRADE_ReturnOrderItem(
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
GO
