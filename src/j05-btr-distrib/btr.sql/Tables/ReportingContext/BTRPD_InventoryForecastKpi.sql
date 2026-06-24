CREATE TABLE BTRPD_InventoryForecastKpi
(
    SnapshotKey                     VARCHAR(10)    NOT NULL CONSTRAINT DF_BTRPD_InventoryForecastKpi_SnapshotKey DEFAULT('CURRENT'),
    GeneratedAt                       DATETIME       NOT NULL CONSTRAINT DF_BTRPD_InventoryForecastKpi_GeneratedAt DEFAULT('3000-01-01'),
    BusinessDate                      DATETIME       NOT NULL CONSTRAINT DF_BTRPD_InventoryForecastKpi_BusinessDate DEFAULT('3000-01-01'),
    PlanningHorizonDays               INT            NOT NULL CONSTRAINT DF_BTRPD_InventoryForecastKpi_PlanningHorizonDays DEFAULT(0),
    CurrentInventoryValue             DECIMAL(18,2)  NOT NULL CONSTRAINT DF_BTRPD_InventoryForecastKpi_CurrentInventoryValue DEFAULT(0),
    ProjectedInventoryValue           DECIMAL(18,2)  NOT NULL CONSTRAINT DF_BTRPD_InventoryForecastKpi_ProjectedInventoryValue DEFAULT(0),
    BestCaseProjectedValue            DECIMAL(18,2)  NOT NULL CONSTRAINT DF_BTRPD_InventoryForecastKpi_BestCaseProjectedValue DEFAULT(0),
    WorstCaseProjectedValue           DECIMAL(18,2)  NOT NULL CONSTRAINT DF_BTRPD_InventoryForecastKpi_WorstCaseProjectedValue DEFAULT(0),
    AverageDailyConsumptionUnits      DECIMAL(18,4)  NOT NULL CONSTRAINT DF_BTRPD_InventoryForecastKpi_AverageDailyConsumptionUnits DEFAULT(0),
    WeightedAverageDaysOfSupply       DECIMAL(18,2)  NULL,
    UnderstockValue                   DECIMAL(18,2)  NOT NULL CONSTRAINT DF_BTRPD_InventoryForecastKpi_UnderstockValue DEFAULT(0),
    OverstockValue                    DECIMAL(18,2)  NOT NULL CONSTRAINT DF_BTRPD_InventoryForecastKpi_OverstockValue DEFAULT(0),
    StockOutRiskItemCount             INT            NOT NULL CONSTRAINT DF_BTRPD_InventoryForecastKpi_StockOutRiskItemCount DEFAULT(0),
    InventoryCoveragePercent          DECIMAL(9,4)   NULL,
    InventoryTurnoverForecast         DECIMAL(9,4)   NULL,
    InventoryHealthScore              INT            NOT NULL CONSTRAINT DF_BTRPD_InventoryForecastKpi_InventoryHealthScore DEFAULT(0),
    ForecastConfidence                VARCHAR(10)    NOT NULL CONSTRAINT DF_BTRPD_InventoryForecastKpi_ForecastConfidence DEFAULT(''),
    AtRiskInventoryPercent            DECIMAL(9,4)   NULL,
    ForecastConsumptionUnits          DECIMAL(18,4)  NOT NULL CONSTRAINT DF_BTRPD_InventoryForecastKpi_ForecastConsumptionUnits DEFAULT(0),
    HeatCellLowLow                    INT            NOT NULL CONSTRAINT DF_BTRPD_InventoryForecastKpi_HeatCellLowLow DEFAULT(0),
    HeatCellLowMed                    INT            NOT NULL CONSTRAINT DF_BTRPD_InventoryForecastKpi_HeatCellLowMed DEFAULT(0),
    HeatCellLowHigh                   INT            NOT NULL CONSTRAINT DF_BTRPD_InventoryForecastKpi_HeatCellLowHigh DEFAULT(0),
    HeatCellMedLow                    INT            NOT NULL CONSTRAINT DF_BTRPD_InventoryForecastKpi_HeatCellMedLow DEFAULT(0),
    HeatCellMedMed                    INT            NOT NULL CONSTRAINT DF_BTRPD_InventoryForecastKpi_HeatCellMedMed DEFAULT(0),
    HeatCellMedHigh                   INT            NOT NULL CONSTRAINT DF_BTRPD_InventoryForecastKpi_HeatCellMedHigh DEFAULT(0),
    HeatCellHighLow                   INT            NOT NULL CONSTRAINT DF_BTRPD_InventoryForecastKpi_HeatCellHighLow DEFAULT(0),
    HeatCellHighMed                   INT            NOT NULL CONSTRAINT DF_BTRPD_InventoryForecastKpi_HeatCellHighMed DEFAULT(0),
    HeatCellHighHigh                  INT            NOT NULL CONSTRAINT DF_BTRPD_InventoryForecastKpi_HeatCellHighHigh DEFAULT(0),
    LastRefreshLogId                  VARCHAR(26)    NOT NULL CONSTRAINT DF_BTRPD_InventoryForecastKpi_LastRefreshLogId DEFAULT(''),

    CONSTRAINT PK_BTRPD_InventoryForecastKpi PRIMARY KEY CLUSTERED (SnapshotKey)
)
GO
