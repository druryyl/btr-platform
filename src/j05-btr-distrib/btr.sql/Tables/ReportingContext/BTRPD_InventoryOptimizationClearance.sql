CREATE TABLE BTRPD_InventoryOptimizationClearance
(
    InventoryOptimizationClearanceId VARCHAR(26)    NOT NULL CONSTRAINT DF_BTRPD_InventoryOptimizationClearance_Id DEFAULT(''),
    SnapshotKey                      VARCHAR(10)    NOT NULL CONSTRAINT DF_BTRPD_InventoryOptimizationClearance_SnapshotKey DEFAULT('CURRENT'),
    SortOrder                        INT            NOT NULL CONSTRAINT DF_BTRPD_InventoryOptimizationClearance_SortOrder DEFAULT(0),
    PriorityScore                    INT            NOT NULL CONSTRAINT DF_BTRPD_InventoryOptimizationClearance_PriorityScore DEFAULT(0),
    Category                         VARCHAR(20)    NOT NULL CONSTRAINT DF_BTRPD_InventoryOptimizationClearance_Category DEFAULT(''),
    BrgId                            VARCHAR(26)    NOT NULL CONSTRAINT DF_BTRPD_InventoryOptimizationClearance_BrgId DEFAULT(''),
    BrgName                          VARCHAR(200)   NOT NULL CONSTRAINT DF_BTRPD_InventoryOptimizationClearance_BrgName DEFAULT(''),
    InventoryValueIdr                DECIMAL(18,2)  NOT NULL CONSTRAINT DF_BTRPD_InventoryOptimizationClearance_InventoryValueIdr DEFAULT(0),
    IdleDays                         INT            NULL,
    RecommendedAction                VARCHAR(80)    NOT NULL CONSTRAINT DF_BTRPD_InventoryOptimizationClearance_RecommendedAction DEFAULT(''),
    ReasonText                       VARCHAR(500)   NOT NULL CONSTRAINT DF_BTRPD_InventoryOptimizationClearance_ReasonText DEFAULT(''),
    RuleId                           VARCHAR(20)    NOT NULL CONSTRAINT DF_BTRPD_InventoryOptimizationClearance_RuleId DEFAULT(''),
    ReportRoute                      VARCHAR(200)   NOT NULL CONSTRAINT DF_BTRPD_InventoryOptimizationClearance_ReportRoute DEFAULT(''),
    DrillDownRoute                   VARCHAR(200)   NOT NULL CONSTRAINT DF_BTRPD_InventoryOptimizationClearance_DrillDownRoute DEFAULT(''),

    CONSTRAINT PK_BTRPD_InventoryOptimizationClearance PRIMARY KEY CLUSTERED (InventoryOptimizationClearanceId)
)
GO

CREATE INDEX IX_BTRPD_InventoryOptimizationClearance_SnapshotKey_SortOrder
    ON BTRPD_InventoryOptimizationClearance (SnapshotKey, SortOrder)
GO
