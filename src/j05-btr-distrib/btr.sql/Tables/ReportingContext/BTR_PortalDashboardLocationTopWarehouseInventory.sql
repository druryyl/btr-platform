CREATE TABLE BTR_PortalDashboardLocationTopWarehouseInventory
(
    LocationTopWarehouseInventoryId VARCHAR(13)   NOT NULL CONSTRAINT DF_BTR_PortalDashboardLocationTopWarehouseInventory_LocationTopWarehouseInventoryId DEFAULT(''),
    SnapshotKey                     VARCHAR(10)   NOT NULL CONSTRAINT DF_BTR_PortalDashboardLocationTopWarehouseInventory_SnapshotKey DEFAULT('CURRENT'),
    Rank                            INT           NOT NULL CONSTRAINT DF_BTR_PortalDashboardLocationTopWarehouseInventory_Rank DEFAULT(0),
    WarehouseId                     VARCHAR(5)    NOT NULL CONSTRAINT DF_BTR_PortalDashboardLocationTopWarehouseInventory_WarehouseId DEFAULT(''),
    WarehouseName                   VARCHAR(50)   NOT NULL CONSTRAINT DF_BTR_PortalDashboardLocationTopWarehouseInventory_WarehouseName DEFAULT(''),
    InventoryValue                  DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTR_PortalDashboardLocationTopWarehouseInventory_InventoryValue DEFAULT(0),
    PercentOfTotal                  DECIMAL(9,4)  NULL,
    ReportRoute                     VARCHAR(100)  NULL,

    CONSTRAINT PK_BTR_PortalDashboardLocationTopWarehouseInventory PRIMARY KEY CLUSTERED (LocationTopWarehouseInventoryId),
    CONSTRAINT UX_BTR_PortalDashboardLocationTopWarehouseInventory_SnapshotKey_Rank UNIQUE (SnapshotKey, Rank)
)
GO
