CREATE TABLE BTR_PortalDashboardInventoryBreakdown
(
    InventoryBreakdownId VARCHAR(13)   NOT NULL CONSTRAINT DF_BTR_PortalDashboardInventoryBreakdown_InventoryBreakdownId DEFAULT(''),
    SnapshotKey          VARCHAR(10)   NOT NULL CONSTRAINT DF_BTR_PortalDashboardInventoryBreakdown_SnapshotKey DEFAULT('CURRENT'),
    DimensionType        VARCHAR(10)   NOT NULL CONSTRAINT DF_BTR_PortalDashboardInventoryBreakdown_DimensionType DEFAULT(''),
    Name                 VARCHAR(50)   NOT NULL CONSTRAINT DF_BTR_PortalDashboardInventoryBreakdown_Name DEFAULT(''),
    InventoryValue       DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTR_PortalDashboardInventoryBreakdown_InventoryValue DEFAULT(0),
    IsTop10              BIT           NOT NULL CONSTRAINT DF_BTR_PortalDashboardInventoryBreakdown_IsTop10 DEFAULT(0),
    Top10Rank            INT           NULL,

    CONSTRAINT PK_BTR_PortalDashboardInventoryBreakdown PRIMARY KEY CLUSTERED (InventoryBreakdownId)
)
GO

CREATE INDEX IX_BTR_PortalDashboardInventoryBreakdown_SnapshotKey_DimensionType
    ON BTR_PortalDashboardInventoryBreakdown (SnapshotKey, DimensionType)
GO
