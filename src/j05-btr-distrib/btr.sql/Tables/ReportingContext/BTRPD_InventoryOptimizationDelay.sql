CREATE TABLE BTRPD_InventoryOptimizationDelay
(
    InventoryOptimizationDelayId VARCHAR(26)    NOT NULL CONSTRAINT DF_BTRPD_InventoryOptimizationDelay_Id DEFAULT(''),
    SnapshotKey                  VARCHAR(10)    NOT NULL CONSTRAINT DF_BTRPD_InventoryOptimizationDelay_SnapshotKey DEFAULT('CURRENT'),
    SortOrder                    INT            NOT NULL CONSTRAINT DF_BTRPD_InventoryOptimizationDelay_SortOrder DEFAULT(0),
    PriorityScore                INT            NOT NULL CONSTRAINT DF_BTRPD_InventoryOptimizationDelay_PriorityScore DEFAULT(0),
    Category                     VARCHAR(20)    NOT NULL CONSTRAINT DF_BTRPD_InventoryOptimizationDelay_Category DEFAULT(''),
    ActionType                   VARCHAR(40)    NOT NULL CONSTRAINT DF_BTRPD_InventoryOptimizationDelay_ActionType DEFAULT(''),
    ActionLabel                  VARCHAR(80)    NOT NULL CONSTRAINT DF_BTRPD_InventoryOptimizationDelay_ActionLabel DEFAULT(''),
    BrgId                        VARCHAR(26)    NOT NULL CONSTRAINT DF_BTRPD_InventoryOptimizationDelay_BrgId DEFAULT(''),
    BrgName                      VARCHAR(200)   NOT NULL CONSTRAINT DF_BTRPD_InventoryOptimizationDelay_BrgName DEFAULT(''),
    SupplierName                 VARCHAR(200)   NOT NULL CONSTRAINT DF_BTRPD_InventoryOptimizationDelay_SupplierName DEFAULT(''),
    DaysOfSupply                 DECIMAL(18,2)  NULL,
    MovementClass                VARCHAR(30)    NOT NULL CONSTRAINT DF_BTRPD_InventoryOptimizationDelay_MovementClass DEFAULT(''),
    SuggestedQty                 DECIMAL(18,4)  NULL,
    ReasonText                   VARCHAR(500)   NOT NULL CONSTRAINT DF_BTRPD_InventoryOptimizationDelay_ReasonText DEFAULT(''),
    RuleId                       VARCHAR(20)    NOT NULL CONSTRAINT DF_BTRPD_InventoryOptimizationDelay_RuleId DEFAULT(''),
    ReportRoute                  VARCHAR(200)   NOT NULL CONSTRAINT DF_BTRPD_InventoryOptimizationDelay_ReportRoute DEFAULT(''),
    DrillDownRoute               VARCHAR(200)   NOT NULL CONSTRAINT DF_BTRPD_InventoryOptimizationDelay_DrillDownRoute DEFAULT(''),

    CONSTRAINT PK_BTRPD_InventoryOptimizationDelay PRIMARY KEY CLUSTERED (InventoryOptimizationDelayId)
)
GO

CREATE INDEX IX_BTRPD_InventoryOptimizationDelay_SnapshotKey_SortOrder
    ON BTRPD_InventoryOptimizationDelay (SnapshotKey, SortOrder)
GO
