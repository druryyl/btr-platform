CREATE TABLE BTRPD_LocationKpi

(

    SnapshotKey                        VARCHAR(10)   NOT NULL CONSTRAINT DF_BTRPD_LocationKpi_SnapshotKey DEFAULT('CURRENT'),

    GeneratedAt                        DATETIME      NOT NULL CONSTRAINT DF_BTRPD_LocationKpi_GeneratedAt DEFAULT('3000-01-01'),

    PeriodYear                         INT           NOT NULL CONSTRAINT DF_BTRPD_LocationKpi_PeriodYear DEFAULT(0),

    PeriodMonth                        INT           NOT NULL CONSTRAINT DF_BTRPD_LocationKpi_PeriodMonth DEFAULT(0),

    Top1WarehouseInventoryPercent      DECIMAL(9,4)  NULL,

    Top3WarehouseInventoryPercent      DECIMAL(9,4)  NULL,

    Top1WarehouseAtRiskPercent         DECIMAL(9,4)  NULL,

    Top1WarehouseSalesPercent          DECIMAL(9,4)  NULL,

    Top1WilayahSalesPercent            DECIMAL(9,4)  NULL,

    InactiveWarehouseWithStockCount    INT           NOT NULL CONSTRAINT DF_BTRPD_LocationKpi_InactiveWarehouseWithStockCount DEFAULT(0),

    WarehouseNoSalesWithInventoryCount INT           NOT NULL CONSTRAINT DF_BTRPD_LocationKpi_WarehouseNoSalesWithInventoryCount DEFAULT(0),

    TotalInventoryValue                DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTRPD_LocationKpi_TotalInventoryValue DEFAULT(0),

    TotalAtRiskValue                   DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTRPD_LocationKpi_TotalAtRiskValue DEFAULT(0),

    TotalOmzet                         DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTRPD_LocationKpi_TotalOmzet DEFAULT(0),

    TotalPurchase                      DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTRPD_LocationKpi_TotalPurchase DEFAULT(0),

    LastRefreshLogId VARCHAR(26)   NOT NULL CONSTRAINT DF_BTRPD_LocationKpi_LastRefreshLogId DEFAULT(''),



    CONSTRAINT PK_BTRPD_LocationKpi PRIMARY KEY CLUSTERED (SnapshotKey)

)

GO

