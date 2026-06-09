CREATE TABLE BTR_PortalDashboardLocationTopWarehouseSales
(
    LocationTopWarehouseSalesId VARCHAR(13)   NOT NULL CONSTRAINT DF_BTR_PortalDashboardLocationTopWarehouseSales_LocationTopWarehouseSalesId DEFAULT(''),
    SnapshotKey                 VARCHAR(10)   NOT NULL CONSTRAINT DF_BTR_PortalDashboardLocationTopWarehouseSales_SnapshotKey DEFAULT('CURRENT'),
    Rank                        INT           NOT NULL CONSTRAINT DF_BTR_PortalDashboardLocationTopWarehouseSales_Rank DEFAULT(0),
    WarehouseId                 VARCHAR(5)    NOT NULL CONSTRAINT DF_BTR_PortalDashboardLocationTopWarehouseSales_WarehouseId DEFAULT(''),
    WarehouseName               VARCHAR(50)   NOT NULL CONSTRAINT DF_BTR_PortalDashboardLocationTopWarehouseSales_WarehouseName DEFAULT(''),
    MtdOmzet                    DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTR_PortalDashboardLocationTopWarehouseSales_MtdOmzet DEFAULT(0),
    PercentOfTotal              DECIMAL(9,4)  NULL,
    ReportRoute                 VARCHAR(100)  NULL,

    CONSTRAINT PK_BTR_PortalDashboardLocationTopWarehouseSales PRIMARY KEY CLUSTERED (LocationTopWarehouseSalesId),
    CONSTRAINT UX_BTR_PortalDashboardLocationTopWarehouseSales_SnapshotKey_Rank UNIQUE (SnapshotKey, Rank)
)
GO
