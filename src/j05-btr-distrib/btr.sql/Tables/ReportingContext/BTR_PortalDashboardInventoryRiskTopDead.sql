CREATE TABLE BTR_PortalDashboardInventoryRiskTopDead
(
    InventoryRiskTopDeadId VARCHAR(13)   NOT NULL CONSTRAINT DF_BTR_PortalDashboardInventoryRiskTopDead_InventoryRiskTopDeadId DEFAULT(''),
    SnapshotKey            VARCHAR(10)   NOT NULL CONSTRAINT DF_BTR_PortalDashboardInventoryRiskTopDead_SnapshotKey DEFAULT('CURRENT'),
    Rank                   INT           NOT NULL CONSTRAINT DF_BTR_PortalDashboardInventoryRiskTopDead_Rank DEFAULT(0),
    BrgId                  VARCHAR(13)   NOT NULL CONSTRAINT DF_BTR_PortalDashboardInventoryRiskTopDead_BrgId DEFAULT(''),
    BrgCode                VARCHAR(20)   NOT NULL CONSTRAINT DF_BTR_PortalDashboardInventoryRiskTopDead_BrgCode DEFAULT(''),
    BrgName                VARCHAR(100)  NOT NULL CONSTRAINT DF_BTR_PortalDashboardInventoryRiskTopDead_BrgName DEFAULT(''),
    KategoriName           VARCHAR(50)   NOT NULL CONSTRAINT DF_BTR_PortalDashboardInventoryRiskTopDead_KategoriName DEFAULT(''),
    SupplierName           VARCHAR(50)   NOT NULL CONSTRAINT DF_BTR_PortalDashboardInventoryRiskTopDead_SupplierName DEFAULT(''),
    Qty                    INT           NOT NULL CONSTRAINT DF_BTR_PortalDashboardInventoryRiskTopDead_Qty DEFAULT(0),
    InventoryValue         DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTR_PortalDashboardInventoryRiskTopDead_InventoryValue DEFAULT(0),
    DaysSinceLastFaktur    INT           NOT NULL CONSTRAINT DF_BTR_PortalDashboardInventoryRiskTopDead_DaysSinceLastFaktur DEFAULT(0),
    PercentOfAtRisk        DECIMAL(9,4)  NULL,

    CONSTRAINT PK_BTR_PortalDashboardInventoryRiskTopDead PRIMARY KEY CLUSTERED (InventoryRiskTopDeadId)
)
GO

CREATE UNIQUE INDEX UX_BTR_PortalDashboardInventoryRiskTopDead_SnapshotKey_Rank
    ON BTR_PortalDashboardInventoryRiskTopDead (SnapshotKey, Rank)
GO
