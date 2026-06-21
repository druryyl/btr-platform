CREATE TABLE BTRPD_InventoryForecastDailyConsumption
(
    InventoryForecastDailyConsumptionId VARCHAR(26)   NOT NULL CONSTRAINT DF_BTRPD_InventoryForecastDailyConsumption_Id DEFAULT(''),
    SnapshotKey                         VARCHAR(10)   NOT NULL CONSTRAINT DF_BTRPD_InventoryForecastDailyConsumption_SnapshotKey DEFAULT('CURRENT'),
    ConsumptionDate                     DATETIME      NOT NULL CONSTRAINT DF_BTRPD_InventoryForecastDailyConsumption_ConsumptionDate DEFAULT('3000-01-01'),
    DayIndex                            INT           NOT NULL CONSTRAINT DF_BTRPD_InventoryForecastDailyConsumption_DayIndex DEFAULT(0),
    UnitsSold                           DECIMAL(18,4) NOT NULL CONSTRAINT DF_BTRPD_InventoryForecastDailyConsumption_UnitsSold DEFAULT(0),
    AdcReference                        DECIMAL(18,4) NOT NULL CONSTRAINT DF_BTRPD_InventoryForecastDailyConsumption_AdcReference DEFAULT(0),

    CONSTRAINT PK_BTRPD_InventoryForecastDailyConsumption PRIMARY KEY CLUSTERED (InventoryForecastDailyConsumptionId)
)
GO

CREATE INDEX IX_BTRPD_InventoryForecastDailyConsumption_SnapshotKey_Date
    ON BTRPD_InventoryForecastDailyConsumption (SnapshotKey, ConsumptionDate)
GO
