CREATE TABLE BTR_PortalDashboardInventoryKpi
(
    SnapshotKey          VARCHAR(10)   NOT NULL CONSTRAINT DF_BTR_PortalDashboardInventoryKpi_SnapshotKey DEFAULT('CURRENT'),
    GeneratedAt          DATETIME      NOT NULL CONSTRAINT DF_BTR_PortalDashboardInventoryKpi_GeneratedAt DEFAULT('3000-01-01'),
    TotalInventoryValue  DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTR_PortalDashboardInventoryKpi_TotalInventoryValue DEFAULT(0),
    TotalItem            INT           NOT NULL CONSTRAINT DF_BTR_PortalDashboardInventoryKpi_TotalItem DEFAULT(0),
    LastRefreshLogId     VARCHAR(13)   NOT NULL CONSTRAINT DF_BTR_PortalDashboardInventoryKpi_LastRefreshLogId DEFAULT(''),

    CONSTRAINT PK_BTR_PortalDashboardInventoryKpi PRIMARY KEY CLUSTERED (SnapshotKey)
)
GO
