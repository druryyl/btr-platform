CREATE TABLE BTRPD_LocationTopWarehouseInventory

(

    LocationTopWarehouseInventoryId VARCHAR(26)   NOT NULL CONSTRAINT DF_BTRPD_LocationTopWarehouseInventory_LocationTopWarehouseInventoryId DEFAULT(''),

    SnapshotKey                     VARCHAR(10)   NOT NULL CONSTRAINT DF_BTRPD_LocationTopWarehouseInventory_SnapshotKey DEFAULT('CURRENT'),

    Rank                            INT           NOT NULL CONSTRAINT DF_BTRPD_LocationTopWarehouseInventory_Rank DEFAULT(0),

    WarehouseId                     VARCHAR(5)    NOT NULL CONSTRAINT DF_BTRPD_LocationTopWarehouseInventory_WarehouseId DEFAULT(''),

    WarehouseName                   VARCHAR(50)   NOT NULL CONSTRAINT DF_BTRPD_LocationTopWarehouseInventory_WarehouseName DEFAULT(''),

    InventoryValue                  DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTRPD_LocationTopWarehouseInventory_InventoryValue DEFAULT(0),

    PercentOfTotal                  DECIMAL(9,4)  NULL,

    ReportRoute                     VARCHAR(100)  NULL,



    CONSTRAINT PK_BTRPD_LocationTopWarehouseInventory PRIMARY KEY CLUSTERED (LocationTopWarehouseInventoryId),

    CONSTRAINT UX_BTRPD_LocationTopWarehouseInventory_SnapshotKey_Rank UNIQUE (SnapshotKey, Rank)

)

GO

