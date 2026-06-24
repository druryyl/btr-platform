CREATE TABLE BTRPD_InventoryOptimizationTransfer
(
    InventoryOptimizationTransferId VARCHAR(26)    NOT NULL CONSTRAINT DF_BTRPD_InventoryOptimizationTransfer_Id DEFAULT(''),
    SnapshotKey                     VARCHAR(10)    NOT NULL CONSTRAINT DF_BTRPD_InventoryOptimizationTransfer_SnapshotKey DEFAULT('CURRENT'),
    SortOrder                       INT            NOT NULL CONSTRAINT DF_BTRPD_InventoryOptimizationTransfer_SortOrder DEFAULT(0),
    PriorityScore                   INT            NOT NULL CONSTRAINT DF_BTRPD_InventoryOptimizationTransfer_PriorityScore DEFAULT(0),
    Category                        VARCHAR(20)    NOT NULL CONSTRAINT DF_BTRPD_InventoryOptimizationTransfer_Category DEFAULT(''),
    BrgId                           VARCHAR(26)    NOT NULL CONSTRAINT DF_BTRPD_InventoryOptimizationTransfer_BrgId DEFAULT(''),
    BrgName                         VARCHAR(200)   NOT NULL CONSTRAINT DF_BTRPD_InventoryOptimizationTransfer_BrgName DEFAULT(''),
    WarehouseFromId                 VARCHAR(5)     NOT NULL CONSTRAINT DF_BTRPD_InventoryOptimizationTransfer_WarehouseFromId DEFAULT(''),
    WarehouseFromName               VARCHAR(50)    NOT NULL CONSTRAINT DF_BTRPD_InventoryOptimizationTransfer_WarehouseFromName DEFAULT(''),
    WarehouseToId                   VARCHAR(5)     NOT NULL CONSTRAINT DF_BTRPD_InventoryOptimizationTransfer_WarehouseToId DEFAULT(''),
    WarehouseToName                 VARCHAR(50)    NOT NULL CONSTRAINT DF_BTRPD_InventoryOptimizationTransfer_WarehouseToName DEFAULT(''),
    TransferQty                     DECIMAL(18,4)  NOT NULL CONSTRAINT DF_BTRPD_InventoryOptimizationTransfer_TransferQty DEFAULT(0),
    DestDaysOfSupply                DECIMAL(18,2)  NULL,
    ReasonText                      VARCHAR(500)   NOT NULL CONSTRAINT DF_BTRPD_InventoryOptimizationTransfer_ReasonText DEFAULT(''),
    RuleId                          VARCHAR(20)    NOT NULL CONSTRAINT DF_BTRPD_InventoryOptimizationTransfer_RuleId DEFAULT(''),
    ReportRoute                     VARCHAR(200)   NOT NULL CONSTRAINT DF_BTRPD_InventoryOptimizationTransfer_ReportRoute DEFAULT(''),
    DrillDownRoute                  VARCHAR(200)   NOT NULL CONSTRAINT DF_BTRPD_InventoryOptimizationTransfer_DrillDownRoute DEFAULT(''),

    CONSTRAINT PK_BTRPD_InventoryOptimizationTransfer PRIMARY KEY CLUSTERED (InventoryOptimizationTransferId)
)
GO

CREATE INDEX IX_BTRPD_InventoryOptimizationTransfer_SnapshotKey_SortOrder
    ON BTRPD_InventoryOptimizationTransfer (SnapshotKey, SortOrder)
GO
