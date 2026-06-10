CREATE TABLE BTRPD_InventoryRiskAging
(
    InventoryRiskAgingId VARCHAR(26)   NOT NULL CONSTRAINT DF_BTRPD_InventoryRiskAging_InventoryRiskAgingId DEFAULT(''),
    SnapshotKey          VARCHAR(10)   NOT NULL CONSTRAINT DF_BTRPD_InventoryRiskAging_SnapshotKey DEFAULT('CURRENT'),
    BucketKey            VARCHAR(20)   NOT NULL CONSTRAINT DF_BTRPD_InventoryRiskAging_BucketKey DEFAULT(''),
    BucketLabel          VARCHAR(50)   NOT NULL CONSTRAINT DF_BTRPD_InventoryRiskAging_BucketLabel DEFAULT(''),
    InventoryValue       DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTRPD_InventoryRiskAging_InventoryValue DEFAULT(0),
    ItemCount            INT           NOT NULL CONSTRAINT DF_BTRPD_InventoryRiskAging_ItemCount DEFAULT(0),
    SortOrder            INT           NOT NULL CONSTRAINT DF_BTRPD_InventoryRiskAging_SortOrder DEFAULT(0),

    CONSTRAINT PK_BTRPD_InventoryRiskAging PRIMARY KEY CLUSTERED (InventoryRiskAgingId)
)
GO

CREATE UNIQUE INDEX UX_BTRPD_InventoryRiskAging_SnapshotKey_BucketKey
    ON BTRPD_InventoryRiskAging (SnapshotKey, BucketKey)
GO
