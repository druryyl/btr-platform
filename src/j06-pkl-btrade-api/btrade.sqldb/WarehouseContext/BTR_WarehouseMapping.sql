-- create table for the centralized Warehouse Mapping (Architecture §6.3, GAP-012/ADR-RO-008)
CREATE TABLE BTR_WarehouseMapping(
    WarehouseCode VARCHAR(20) NOT NULL CONSTRAINT DF_BTR_WarehouseMapping_WarehouseCode DEFAULT(''),
    ServerId      VARCHAR(5)  NOT NULL CONSTRAINT DF_BTR_WarehouseMapping_ServerId      DEFAULT(''),

    CONSTRAINT PK_BTR_WarehouseMapping PRIMARY KEY CLUSTERED (WarehouseCode)
)
GO
