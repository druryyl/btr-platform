CREATE TABLE BTR_PortalDashboardInventoryRiskKpi
(
    SnapshotKey              VARCHAR(10)   NOT NULL CONSTRAINT DF_BTR_PortalDashboardInventoryRiskKpi_SnapshotKey DEFAULT('CURRENT'),
    GeneratedAt              DATETIME      NOT NULL CONSTRAINT DF_BTR_PortalDashboardInventoryRiskKpi_GeneratedAt DEFAULT('3000-01-01'),
    TotalInventoryValue      DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTR_PortalDashboardInventoryRiskKpi_TotalInventoryValue DEFAULT(0),
    TotalItem                INT           NOT NULL CONSTRAINT DF_BTR_PortalDashboardInventoryRiskKpi_TotalItem DEFAULT(0),
    DeadStockItemCount       INT           NOT NULL CONSTRAINT DF_BTR_PortalDashboardInventoryRiskKpi_DeadStockItemCount DEFAULT(0),
    DeadStockValue           DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTR_PortalDashboardInventoryRiskKpi_DeadStockValue DEFAULT(0),
    SlowMovingItemCount      INT           NOT NULL CONSTRAINT DF_BTR_PortalDashboardInventoryRiskKpi_SlowMovingItemCount DEFAULT(0),
    SlowMovingValue          DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTR_PortalDashboardInventoryRiskKpi_SlowMovingValue DEFAULT(0),
    NeverSoldItemCount       INT           NOT NULL CONSTRAINT DF_BTR_PortalDashboardInventoryRiskKpi_NeverSoldItemCount DEFAULT(0),
    NeverSoldValue           DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTR_PortalDashboardInventoryRiskKpi_NeverSoldValue DEFAULT(0),
    AtRiskInventoryValue     DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTR_PortalDashboardInventoryRiskKpi_AtRiskInventoryValue DEFAULT(0),
    AtRiskInventoryPercent   DECIMAL(9,4)  NULL,
    RequiresAttention        BIT           NOT NULL CONSTRAINT DF_BTR_PortalDashboardInventoryRiskKpi_RequiresAttention DEFAULT(0),
    LastRefreshLogId         VARCHAR(13)   NOT NULL CONSTRAINT DF_BTR_PortalDashboardInventoryRiskKpi_LastRefreshLogId DEFAULT(''),

    CONSTRAINT PK_BTR_PortalDashboardInventoryRiskKpi PRIMARY KEY CLUSTERED (SnapshotKey)
)
GO
