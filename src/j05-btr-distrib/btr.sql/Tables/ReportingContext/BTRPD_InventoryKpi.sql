CREATE TABLE BTRPD_InventoryKpi
(
    SnapshotKey          VARCHAR(10)   NOT NULL CONSTRAINT DF_BTRPD_InventoryKpi_SnapshotKey DEFAULT('CURRENT'),
    GeneratedAt          DATETIME      NOT NULL CONSTRAINT DF_BTRPD_InventoryKpi_GeneratedAt DEFAULT('3000-01-01'),
    TotalInventoryValue  DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTRPD_InventoryKpi_TotalInventoryValue DEFAULT(0),
    TotalItem            INT           NOT NULL CONSTRAINT DF_BTRPD_InventoryKpi_TotalItem DEFAULT(0),
    LastRefreshLogId VARCHAR(26)   NOT NULL CONSTRAINT DF_BTRPD_InventoryKpi_LastRefreshLogId DEFAULT(''),

    CONSTRAINT PK_BTRPD_InventoryKpi PRIMARY KEY CLUSTERED (SnapshotKey)
)
GO
