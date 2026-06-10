CREATE TABLE BTRPD_InventoryRiskKpi
(
    SnapshotKey              VARCHAR(10)   NOT NULL CONSTRAINT DF_BTRPD_InventoryRiskKpi_SnapshotKey DEFAULT('CURRENT'),
    GeneratedAt              DATETIME      NOT NULL CONSTRAINT DF_BTRPD_InventoryRiskKpi_GeneratedAt DEFAULT('3000-01-01'),
    TotalInventoryValue      DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTRPD_InventoryRiskKpi_TotalInventoryValue DEFAULT(0),
    TotalItem                INT           NOT NULL CONSTRAINT DF_BTRPD_InventoryRiskKpi_TotalItem DEFAULT(0),
    DeadStockItemCount       INT           NOT NULL CONSTRAINT DF_BTRPD_InventoryRiskKpi_DeadStockItemCount DEFAULT(0),
    DeadStockValue           DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTRPD_InventoryRiskKpi_DeadStockValue DEFAULT(0),
    SlowMovingItemCount      INT           NOT NULL CONSTRAINT DF_BTRPD_InventoryRiskKpi_SlowMovingItemCount DEFAULT(0),
    SlowMovingValue          DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTRPD_InventoryRiskKpi_SlowMovingValue DEFAULT(0),
    NeverSoldItemCount       INT           NOT NULL CONSTRAINT DF_BTRPD_InventoryRiskKpi_NeverSoldItemCount DEFAULT(0),
    NeverSoldValue           DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTRPD_InventoryRiskKpi_NeverSoldValue DEFAULT(0),
    AtRiskInventoryValue     DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTRPD_InventoryRiskKpi_AtRiskInventoryValue DEFAULT(0),
    AtRiskInventoryPercent   DECIMAL(9,4)  NULL,
    RequiresAttention        BIT           NOT NULL CONSTRAINT DF_BTRPD_InventoryRiskKpi_RequiresAttention DEFAULT(0),
    LastRefreshLogId VARCHAR(26)   NOT NULL CONSTRAINT DF_BTRPD_InventoryRiskKpi_LastRefreshLogId DEFAULT(''),

    CONSTRAINT PK_BTRPD_InventoryRiskKpi PRIMARY KEY CLUSTERED (SnapshotKey)
)
GO
