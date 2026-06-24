CREATE TABLE BTRPD_InventoryOptimizationActionHeat
(
    InventoryOptimizationActionHeatId VARCHAR(26) NOT NULL CONSTRAINT DF_BTRPD_InventoryOptimizationActionHeat_Id DEFAULT(''),
    SnapshotKey                       VARCHAR(10) NOT NULL CONSTRAINT DF_BTRPD_InventoryOptimizationActionHeat_SnapshotKey DEFAULT('CURRENT'),
    ActionType                        VARCHAR(40) NOT NULL CONSTRAINT DF_BTRPD_InventoryOptimizationActionHeat_ActionType DEFAULT(''),
    ActionLabel                       VARCHAR(80) NOT NULL CONSTRAINT DF_BTRPD_InventoryOptimizationActionHeat_ActionLabel DEFAULT(''),
    Category                          VARCHAR(20) NOT NULL CONSTRAINT DF_BTRPD_InventoryOptimizationActionHeat_Category DEFAULT(''),
    ActionCount                       INT         NOT NULL CONSTRAINT DF_BTRPD_InventoryOptimizationActionHeat_ActionCount DEFAULT(0),

    CONSTRAINT PK_BTRPD_InventoryOptimizationActionHeat PRIMARY KEY CLUSTERED (InventoryOptimizationActionHeatId)
)
GO

CREATE INDEX IX_BTRPD_InventoryOptimizationActionHeat_SnapshotKey
    ON BTRPD_InventoryOptimizationActionHeat (SnapshotKey)
GO
