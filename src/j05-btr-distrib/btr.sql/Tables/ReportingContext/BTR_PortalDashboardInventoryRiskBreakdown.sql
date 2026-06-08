CREATE TABLE BTR_PortalDashboardInventoryRiskBreakdown
(
    InventoryRiskBreakdownId VARCHAR(13)   NOT NULL CONSTRAINT DF_BTR_PortalDashboardInventoryRiskBreakdown_InventoryRiskBreakdownId DEFAULT(''),
    SnapshotKey              VARCHAR(10)   NOT NULL CONSTRAINT DF_BTR_PortalDashboardInventoryRiskBreakdown_SnapshotKey DEFAULT('CURRENT'),
    DimensionType            VARCHAR(20)   NOT NULL CONSTRAINT DF_BTR_PortalDashboardInventoryRiskBreakdown_DimensionType DEFAULT(''),
    Name                     VARCHAR(50)   NOT NULL CONSTRAINT DF_BTR_PortalDashboardInventoryRiskBreakdown_Name DEFAULT(''),
    AtRiskValue              DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTR_PortalDashboardInventoryRiskBreakdown_AtRiskValue DEFAULT(0),
    ItemCount                INT           NOT NULL CONSTRAINT DF_BTR_PortalDashboardInventoryRiskBreakdown_ItemCount DEFAULT(0),
    Rank                     INT           NOT NULL CONSTRAINT DF_BTR_PortalDashboardInventoryRiskBreakdown_Rank DEFAULT(0),
    PercentOfAtRisk          DECIMAL(9,4)  NULL,

    CONSTRAINT PK_BTR_PortalDashboardInventoryRiskBreakdown PRIMARY KEY CLUSTERED (InventoryRiskBreakdownId)
)
GO

CREATE UNIQUE INDEX UX_BTR_PortalDashboardInventoryRiskBreakdown_SnapshotKey_DimensionType_Rank
    ON BTR_PortalDashboardInventoryRiskBreakdown (SnapshotKey, DimensionType, Rank)
GO
