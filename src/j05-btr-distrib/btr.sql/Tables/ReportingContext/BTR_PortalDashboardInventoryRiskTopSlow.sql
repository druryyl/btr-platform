CREATE TABLE BTR_PortalDashboardInventoryRiskTopSlow
(
    InventoryRiskTopSlowId VARCHAR(13)   NOT NULL CONSTRAINT DF_BTR_PortalDashboardInventoryRiskTopSlow_InventoryRiskTopSlowId DEFAULT(''),
    SnapshotKey            VARCHAR(10)   NOT NULL CONSTRAINT DF_BTR_PortalDashboardInventoryRiskTopSlow_SnapshotKey DEFAULT('CURRENT'),
    Rank                   INT           NOT NULL CONSTRAINT DF_BTR_PortalDashboardInventoryRiskTopSlow_Rank DEFAULT(0),
    BrgId                  VARCHAR(13)   NOT NULL CONSTRAINT DF_BTR_PortalDashboardInventoryRiskTopSlow_BrgId DEFAULT(''),
    BrgCode                VARCHAR(20)   NOT NULL CONSTRAINT DF_BTR_PortalDashboardInventoryRiskTopSlow_BrgCode DEFAULT(''),
    BrgName                VARCHAR(100)  NOT NULL CONSTRAINT DF_BTR_PortalDashboardInventoryRiskTopSlow_BrgName DEFAULT(''),
    KategoriName           VARCHAR(50)   NOT NULL CONSTRAINT DF_BTR_PortalDashboardInventoryRiskTopSlow_KategoriName DEFAULT(''),
    SupplierName           VARCHAR(50)   NOT NULL CONSTRAINT DF_BTR_PortalDashboardInventoryRiskTopSlow_SupplierName DEFAULT(''),
    Qty                    INT           NOT NULL CONSTRAINT DF_BTR_PortalDashboardInventoryRiskTopSlow_Qty DEFAULT(0),
    InventoryValue         DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTR_PortalDashboardInventoryRiskTopSlow_InventoryValue DEFAULT(0),
    DaysSinceLastFaktur    INT           NOT NULL CONSTRAINT DF_BTR_PortalDashboardInventoryRiskTopSlow_DaysSinceLastFaktur DEFAULT(0),
    PercentOfAtRisk        DECIMAL(9,4)  NULL,

    CONSTRAINT PK_BTR_PortalDashboardInventoryRiskTopSlow PRIMARY KEY CLUSTERED (InventoryRiskTopSlowId)
)
GO

CREATE UNIQUE INDEX UX_BTR_PortalDashboardInventoryRiskTopSlow_SnapshotKey_Rank
    ON BTR_PortalDashboardInventoryRiskTopSlow (SnapshotKey, Rank)
GO
