CREATE TABLE BTRPD_InventoryOptimizationReorder
(
    InventoryOptimizationReorderId VARCHAR(26)    NOT NULL CONSTRAINT DF_BTRPD_InventoryOptimizationReorder_Id DEFAULT(''),
    SnapshotKey                    VARCHAR(10)    NOT NULL CONSTRAINT DF_BTRPD_InventoryOptimizationReorder_SnapshotKey DEFAULT('CURRENT'),
    SortOrder                      INT            NOT NULL CONSTRAINT DF_BTRPD_InventoryOptimizationReorder_SortOrder DEFAULT(0),
    PriorityScore                  INT            NOT NULL CONSTRAINT DF_BTRPD_InventoryOptimizationReorder_PriorityScore DEFAULT(0),
    Category                       VARCHAR(20)    NOT NULL CONSTRAINT DF_BTRPD_InventoryOptimizationReorder_Category DEFAULT(''),
    BrgId                          VARCHAR(26)    NOT NULL CONSTRAINT DF_BTRPD_InventoryOptimizationReorder_BrgId DEFAULT(''),
    BrgCode                        VARCHAR(50)    NOT NULL CONSTRAINT DF_BTRPD_InventoryOptimizationReorder_BrgCode DEFAULT(''),
    BrgName                        VARCHAR(200)   NOT NULL CONSTRAINT DF_BTRPD_InventoryOptimizationReorder_BrgName DEFAULT(''),
    SupplierName                   VARCHAR(200)   NOT NULL CONSTRAINT DF_BTRPD_InventoryOptimizationReorder_SupplierName DEFAULT(''),
    RecommendedPurchaseQty         DECIMAL(18,4)  NOT NULL CONSTRAINT DF_BTRPD_InventoryOptimizationReorder_RecommendedPurchaseQty DEFAULT(0),
    EstimatedCostIdr               DECIMAL(18,2)  NOT NULL CONSTRAINT DF_BTRPD_InventoryOptimizationReorder_EstimatedCostIdr DEFAULT(0),
    DaysOfSupply                   DECIMAL(18,2)  NULL,
    ReorderDate                    DATETIME       NULL,
    AverageDailyConsumption        DECIMAL(18,4)  NOT NULL CONSTRAINT DF_BTRPD_InventoryOptimizationReorder_AverageDailyConsumption DEFAULT(0),
    CurrentQty                     DECIMAL(18,4)  NOT NULL CONSTRAINT DF_BTRPD_InventoryOptimizationReorder_CurrentQty DEFAULT(0),
    ReasonText                     VARCHAR(500)   NOT NULL CONSTRAINT DF_BTRPD_InventoryOptimizationReorder_ReasonText DEFAULT(''),
    RuleId                         VARCHAR(20)    NOT NULL CONSTRAINT DF_BTRPD_InventoryOptimizationReorder_RuleId DEFAULT(''),
    ReportRoute                    VARCHAR(200)   NOT NULL CONSTRAINT DF_BTRPD_InventoryOptimizationReorder_ReportRoute DEFAULT(''),
    DrillDownRoute                 VARCHAR(200)   NOT NULL CONSTRAINT DF_BTRPD_InventoryOptimizationReorder_DrillDownRoute DEFAULT(''),

    CONSTRAINT PK_BTRPD_InventoryOptimizationReorder PRIMARY KEY CLUSTERED (InventoryOptimizationReorderId)
)
GO

CREATE INDEX IX_BTRPD_InventoryOptimizationReorder_SnapshotKey_SortOrder
    ON BTRPD_InventoryOptimizationReorder (SnapshotKey, SortOrder)
GO
