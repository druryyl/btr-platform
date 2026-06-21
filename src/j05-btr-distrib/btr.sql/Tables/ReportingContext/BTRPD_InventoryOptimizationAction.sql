CREATE TABLE BTRPD_InventoryOptimizationAction
(
    InventoryOptimizationActionId VARCHAR(26)    NOT NULL CONSTRAINT DF_BTRPD_InventoryOptimizationAction_Id DEFAULT(''),
    SnapshotKey                   VARCHAR(10)    NOT NULL CONSTRAINT DF_BTRPD_InventoryOptimizationAction_SnapshotKey DEFAULT('CURRENT'),
    SortOrder                     INT            NOT NULL CONSTRAINT DF_BTRPD_InventoryOptimizationAction_SortOrder DEFAULT(0),
    PriorityScore                 INT            NOT NULL CONSTRAINT DF_BTRPD_InventoryOptimizationAction_PriorityScore DEFAULT(0),
    Category                      VARCHAR(20)    NOT NULL CONSTRAINT DF_BTRPD_InventoryOptimizationAction_Category DEFAULT(''),
    ActionType                    VARCHAR(40)    NOT NULL CONSTRAINT DF_BTRPD_InventoryOptimizationAction_ActionType DEFAULT(''),
    ActionLabel                   VARCHAR(80)    NOT NULL CONSTRAINT DF_BTRPD_InventoryOptimizationAction_ActionLabel DEFAULT(''),
    BrgId                         VARCHAR(26)    NOT NULL CONSTRAINT DF_BTRPD_InventoryOptimizationAction_BrgId DEFAULT(''),
    BrgName                       VARCHAR(200)   NOT NULL CONSTRAINT DF_BTRPD_InventoryOptimizationAction_BrgName DEFAULT(''),
    SupplierName                  VARCHAR(200)   NOT NULL CONSTRAINT DF_BTRPD_InventoryOptimizationAction_SupplierName DEFAULT(''),
    WarehouseFromId               VARCHAR(5)     NULL,
    WarehouseFromName             VARCHAR(50)    NULL,
    WarehouseToId                 VARCHAR(5)     NULL,
    WarehouseToName               VARCHAR(50)    NULL,
    Quantity                      DECIMAL(18,4)  NULL,
    ImpactValueIdr                DECIMAL(18,2)  NOT NULL CONSTRAINT DF_BTRPD_InventoryOptimizationAction_ImpactValueIdr DEFAULT(0),
    DaysOfSupply                  DECIMAL(18,2)  NULL,
    ReasonText                    VARCHAR(500)   NOT NULL CONSTRAINT DF_BTRPD_InventoryOptimizationAction_ReasonText DEFAULT(''),
    RuleId                        VARCHAR(20)    NOT NULL CONSTRAINT DF_BTRPD_InventoryOptimizationAction_RuleId DEFAULT(''),
    ReportRoute                   VARCHAR(200)   NOT NULL CONSTRAINT DF_BTRPD_InventoryOptimizationAction_ReportRoute DEFAULT(''),
    DrillDownRoute                  VARCHAR(200)   NOT NULL CONSTRAINT DF_BTRPD_InventoryOptimizationAction_DrillDownRoute DEFAULT(''),

    CONSTRAINT PK_BTRPD_InventoryOptimizationAction PRIMARY KEY CLUSTERED (InventoryOptimizationActionId)
)
GO

CREATE INDEX IX_BTRPD_InventoryOptimizationAction_SnapshotKey_SortOrder
    ON BTRPD_InventoryOptimizationAction (SnapshotKey, SortOrder)
GO
