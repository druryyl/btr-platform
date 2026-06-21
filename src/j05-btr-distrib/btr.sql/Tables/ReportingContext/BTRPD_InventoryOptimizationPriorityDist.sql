CREATE TABLE BTRPD_InventoryOptimizationPriorityDist
(
    InventoryOptimizationPriorityDistId VARCHAR(26) NOT NULL CONSTRAINT DF_BTRPD_InventoryOptimizationPriorityDist_Id DEFAULT(''),
    SnapshotKey                         VARCHAR(10) NOT NULL CONSTRAINT DF_BTRPD_InventoryOptimizationPriorityDist_SnapshotKey DEFAULT('CURRENT'),
    Category                            VARCHAR(20) NOT NULL CONSTRAINT DF_BTRPD_InventoryOptimizationPriorityDist_Category DEFAULT(''),
    ActionCount                         INT         NOT NULL CONSTRAINT DF_BTRPD_InventoryOptimizationPriorityDist_ActionCount DEFAULT(0),
    SortOrder                           INT         NOT NULL CONSTRAINT DF_BTRPD_InventoryOptimizationPriorityDist_SortOrder DEFAULT(0),

    CONSTRAINT PK_BTRPD_InventoryOptimizationPriorityDist PRIMARY KEY CLUSTERED (InventoryOptimizationPriorityDistId)
)
GO

CREATE INDEX IX_BTRPD_InventoryOptimizationPriorityDist_SnapshotKey_SortOrder
    ON BTRPD_InventoryOptimizationPriorityDist (SnapshotKey, SortOrder)
GO
