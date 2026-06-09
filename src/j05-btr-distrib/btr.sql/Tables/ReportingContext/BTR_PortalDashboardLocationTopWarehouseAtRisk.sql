CREATE TABLE BTR_PortalDashboardLocationTopWarehouseAtRisk
(
    LocationTopWarehouseAtRiskId VARCHAR(13)   NOT NULL CONSTRAINT DF_BTR_PortalDashboardLocationTopWarehouseAtRisk_LocationTopWarehouseAtRiskId DEFAULT(''),
    SnapshotKey                  VARCHAR(10)   NOT NULL CONSTRAINT DF_BTR_PortalDashboardLocationTopWarehouseAtRisk_SnapshotKey DEFAULT('CURRENT'),
    Rank                         INT           NOT NULL CONSTRAINT DF_BTR_PortalDashboardLocationTopWarehouseAtRisk_Rank DEFAULT(0),
    WarehouseId                  VARCHAR(5)    NOT NULL CONSTRAINT DF_BTR_PortalDashboardLocationTopWarehouseAtRisk_WarehouseId DEFAULT(''),
    WarehouseName                VARCHAR(50)   NOT NULL CONSTRAINT DF_BTR_PortalDashboardLocationTopWarehouseAtRisk_WarehouseName DEFAULT(''),
    AtRiskValue                  DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTR_PortalDashboardLocationTopWarehouseAtRisk_AtRiskValue DEFAULT(0),
    PercentOfTotal               DECIMAL(9,4)  NULL,
    ReportRoute                  VARCHAR(100)  NULL,

    CONSTRAINT PK_BTR_PortalDashboardLocationTopWarehouseAtRisk PRIMARY KEY CLUSTERED (LocationTopWarehouseAtRiskId),
    CONSTRAINT UX_BTR_PortalDashboardLocationTopWarehouseAtRisk_SnapshotKey_Rank UNIQUE (SnapshotKey, Rank)
)
GO
