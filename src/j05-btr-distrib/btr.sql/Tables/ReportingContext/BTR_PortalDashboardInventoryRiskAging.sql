CREATE TABLE BTR_PortalDashboardInventoryRiskAging
(
    InventoryRiskAgingId VARCHAR(13)   NOT NULL CONSTRAINT DF_BTR_PortalDashboardInventoryRiskAging_InventoryRiskAgingId DEFAULT(''),
    SnapshotKey          VARCHAR(10)   NOT NULL CONSTRAINT DF_BTR_PortalDashboardInventoryRiskAging_SnapshotKey DEFAULT('CURRENT'),
    BucketKey            VARCHAR(20)   NOT NULL CONSTRAINT DF_BTR_PortalDashboardInventoryRiskAging_BucketKey DEFAULT(''),
    BucketLabel          VARCHAR(50)   NOT NULL CONSTRAINT DF_BTR_PortalDashboardInventoryRiskAging_BucketLabel DEFAULT(''),
    InventoryValue       DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTR_PortalDashboardInventoryRiskAging_InventoryValue DEFAULT(0),
    ItemCount            INT           NOT NULL CONSTRAINT DF_BTR_PortalDashboardInventoryRiskAging_ItemCount DEFAULT(0),
    SortOrder            INT           NOT NULL CONSTRAINT DF_BTR_PortalDashboardInventoryRiskAging_SortOrder DEFAULT(0),

    CONSTRAINT PK_BTR_PortalDashboardInventoryRiskAging PRIMARY KEY CLUSTERED (InventoryRiskAgingId)
)
GO

CREATE UNIQUE INDEX UX_BTR_PortalDashboardInventoryRiskAging_SnapshotKey_BucketKey
    ON BTR_PortalDashboardInventoryRiskAging (SnapshotKey, BucketKey)
GO
